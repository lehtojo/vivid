BUCKET_SIZE = 1000
MAXIMUM_SLOT_SIZE = 10

MAP_OK = 1
MAP_FAIL = 0
MAP_KEY_DUPLICATION = -1

MapElement<K, V> {
	key: K
	value: V

	init(key: K, value: V) {
		this.key = key
		this.value = value
	}
}

MapBucket<K, V> {
	slots: Array<LinkedList<MapElement<K, V>>>

	init() {
		# Initialize all the slots in this bucket
		slots = Array<LinkedList<MapElement<K, V>>>(BUCKET_SIZE)

		loop (i = 0, i < BUCKET_SIZE, i++) {
			slots[i] = LinkedList<MapElement<K, V>>()
		}
	}

	set(key: K, value: V) {
		# Determine the slot where the value might be stored
		destination = 0

		if compiles { key.hash() } { destination = key.hash() % BUCKET_SIZE }
		else { destination = (key as large) % BUCKET_SIZE }
		if destination < 0 { destination += BUCKET_SIZE }

		slot = slots[destination]

		# Try to find the key from the slot
		loop (iterator = slot.iterator(), iterator, iterator = iterator.next) {
			if iterator.value.key != key continue
			iterator.value.value = value
			=> MAP_OK
		}

		=> MAP_FAIL
	}

	add(key: K, value: V) {
		destination = 0

		if compiles { key.hash() } { destination = key.hash() % BUCKET_SIZE }
		else { destination = (key as large) % BUCKET_SIZE }
		if destination < 0 { destination += BUCKET_SIZE }

		slot = slots[destination]

		# The slot is not allowed to grow past a specific size
		if slot.size() >= MAXIMUM_SLOT_SIZE => MAP_FAIL

		# Two identical keys can not be stored at the same time
		loop (iterator = slot.iterator(), iterator, iterator = iterator.next) {
			if iterator.value.key == key => MAP_KEY_DUPLICATION
		}

		slot.add(MapElement<K, V>(key, value))
		=> MAP_OK
	}

	contains_key(key: K) {
		location = 0

		if compiles { key.hash() } { location = key.hash() % BUCKET_SIZE }
		else { location = (key as large) % BUCKET_SIZE }
		if location < 0 { location += BUCKET_SIZE }
		
		slot = slots[location]
		
		loop (iterator = slot.iterator(), iterator, iterator = iterator.next) {
			if iterator.value.key == key => true
		}

		=> false
	}

	get(key: K) {
		location = 0

		if compiles { key.hash() } { location = key.hash() % BUCKET_SIZE }
		else { location = (key as large) % BUCKET_SIZE }
		if location < 0 { location += BUCKET_SIZE }
		
		slot = slots[location]
		
		loop (iterator = slot.iterator(), iterator, iterator = iterator.next) {
			if iterator.value.key == key {
				=> Optional<V>(iterator.value.value)
			}
		}

		=> Optional<V>()
	}

	remove(key: K) {
		location = 0

		if compiles { key.hash() } { location = key.hash() % BUCKET_SIZE }
		else { location = (key as large) % BUCKET_SIZE }
		if location < 0 { location += BUCKET_SIZE }

		slot = slots[location]

		previous = 0 as LinkedListElement<MapElement<K, V>>

		loop (iterator = slot.iterator(), iterator, iterator = iterator.next) {
			if iterator.value.key == key {
				slot.remove(previous, iterator)
				=> MAP_OK
			}

			previous = iterator
		}

		=> MAP_FAIL
	}

	size() {
		size = 0

		loop (i = 0, i < BUCKET_SIZE, i++) {
			size += slots[i].size()
		}

		=> size
	}
}

MapIterator<K, V> {
	buckets: LinkedList<MapBucket<K, V>>
	bucket: LinkedListElement<MapBucket<K, V>>
	slot: normal
	element: LinkedListElement<MapElement<K, V>>

	init(buckets: LinkedList<MapBucket<K, V>>) {
		this.buckets = buckets
		this.bucket = none as LinkedListElement<MapBucket<K, V>>
	}

	value() => element.value

	private next_element() {
		# Ensure the current bucket is not none
		loop (bucket != none) {
			slot++ # Move to the next slot

			# If the current bucket does not contain the current slot index, move to the next bucket
			if slot >= bucket.value.slots.count {
				slot = -1
				bucket = bucket.next
				continue
			}

			# Load the first element from the current slot and ensure it exists, move to the next slot otherwise
			value = bucket.value.slots[slot].iterator()
			if value == none continue

			element = value
			=> true
		}

		=> false
	}

	next() {
		# At beginning the iterator does not have the first bucket loaded
		if bucket == none {
			bucket = buckets.iterator()
			slot = -1
			=> next_element()
		}
		
		# If the element is none at this point, nothing can be done
		if element == none => false
		
		# Try to access the next element
		element = element.next
		if element != none => true

		# Since the element is none currently, try to get the next element from the next slot
		=> next_element()
	}

	reset() {
		bucket = none
	}
}

Map<K, V> {
	private:
	buckets: LinkedList<MapBucket<K, V>>
	items: List<MapElement<K, V>>

	public:
	init() {
		buckets = LinkedList<MapBucket<K, V>>()
		items = List<MapElement<K, V>>()
		buckets.add(MapBucket<K, V>())
	}

	add(key: K, value: V) {
		loop (iterator = buckets.iterator(), iterator, iterator = iterator.next) {
			result = iterator.value.add(key, value)

			if result == MAP_OK {
				items.add(MapElement<K, V>(key, value))
				=> true
			}

			if result == MAP_KEY_DUPLICATION => false
		}

		bucket = MapBucket<K, V>()
		bucket.add(key, value)
		items.add(MapElement<K, V>(key, value))

		buckets.add(bucket)

		=> true
	}

	set(key: K, value: V) {
		# First try to update the value, if the key has been added already
		loop (iterator = buckets.iterator(), iterator, iterator = iterator.next) {
			result = iterator.value.set(key, value)
			if result == MAP_OK => true
		}

		# Since the map does not contain the key, add the value with the key
		add(key, value)
	}

	contains_key(key: K) {
		loop (bucket = buckets.iterator(), bucket, bucket = bucket.next) {
			if bucket.value.contains_key(key) => true
		}

		=> false
	}

	try_get(key: K) {
		loop (bucket = buckets.iterator(), bucket, bucket = bucket.next) {
			result = bucket.value.get(key)

			if not result.empty => result
		}

		=> Optional<V>()
	}

	get(key: K) {
		loop (bucket = buckets.iterator(), bucket, bucket = bucket.next) {
			result = bucket.value.get(key)

			if not result.empty => result.value
		}

		require(false, 'Map did not contain the specified key')
	}

	remove(key: K) {
		loop (bucket = buckets.iterator(), bucket, bucket = bucket.next) {
			if not bucket.value.remove(key) continue

			# Remove the key and value pair from the items list as well
			loop (i = items.size - 1, i >= 0, i--) {
				if items[i].key != key continue
				items.remove_at(i)
				stop
			}

			=> true
		}

		=> false
	}

	size() {
		size = 0

		loop (bucket = buckets.iterator(), bucket, bucket = bucket.next) {
			size += bucket.value.size()
		}

		=> size
	}

	iterator() {
		=> items.iterator()
	}
}