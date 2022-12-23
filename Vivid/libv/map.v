MIN_MAP_CAPACITY = 5
REMOVED_SLOT_MARKER = -2

export plain KeyValuePair<K, V> {
	key: K
	value: V

	init(key: K, value: V) {
		this.key = key
		this.value = value
	}
}

export plain MapIterator<K, V> {
	slot: MapSlot<K, V>
	slots: MapSlot<K, V>*
	first: normal

	init(slots: MapSlot<K, V>*, first: normal) {
		this.slots = slots
		this.first = first

		if first < 0 return

		slot.key = none as K
		slot.value = none as V
		slot.next = first + 1
		slot.previous = 0
	}

	value() {
		return this as KeyValuePair<K, V> # NOTE: Map slot is identical to a pair
	}

	next() {
		if slot.next <= 0 return false

		index = slot.next - 1
		slot = slots[index]
		return true
	}

	reset() {
		slot.key = none as K
		slot.value = none as V
		slot.next = first + 1
		slot.previous = 0
	}
}

export pack MapSlot<K, V> {
	key: K
	value: V
	next: normal
	previous: normal
}

export Map<K, V> {
	private first: large = -1 # Zero-based index of first slot
	private last: large = -1 # Zero-based index of last slot
	private slots: MapSlot<K, V>* = none as MapSlot<K, V>*
	private capacity: large = 1
	private removed: normal = 0 # Number of removed slots

	readable size: large = 0

	init() {}

	init(capacity: large) {
		rehash(capacity)
	}

	rehash(to: large) {
		if to < MIN_MAP_CAPACITY { to = MIN_MAP_CAPACITY }

		# Save the old slots for iteration
		previous_slots = slots

		# Start from the first slot
		index = first

		# Allocate the new slots
		slots = allocate(to * sizeof(MapSlot<K, V>))
		capacity = to
		first = -1
		last = -1
		size = 0
		removed = 0

		if previous_slots === none return

		loop (index >= 0) {
			slot = previous_slots[index]
			add(slot.key, slot.value) # Add the slot to the new slots

			index = slot.next - 1 # Indices are 1-based
		}

		deallocate(previous_slots)
	}

	add(key: K, value: V) {
		# If the load factor will exceed 50%, rehash the map now
		load_factor = (size + removed + 1) as decimal / capacity

		if load_factor > 0.5 {
			rehash(capacity * 2)
		}

		hash = key as large
		if compiles { key.hash() } { hash = key.hash() }

		attempt = 0

		# Find an available slot by quadratic probing
		loop {
			index = 0
			if attempt < 10 { index = (hash + attempt * attempt) as u64 % capacity }
			else { index = (hash + attempt) as u64 % capacity }

			slot = slots[index]

			# Process occupied slots separately
			if slot.next > 0 or slot.next == -1 {
				# If the slot has the same key, replace the value
				if slot.key == key {
					slot.value = value
					slots[index] = slot
					return
				}

				attempt++
				continue
			}

			# If we allocate a removed slot, decrement the removed count
			if slot.next == REMOVED_SLOT_MARKER {
				removed--
			}

			# Allocate the slot for the specified key and value
			slot.key = key
			slot.value = value
			slot.next = -1
			slot.previous = -1

			if last >= 0 {
				# Connect the last slot to the new slot
				previous = slots[last]
				previous.next = index + 1
				slots[last] = previous

				# Connect the new slot to the last slot
				slot.previous = last + 1

				# Update the index of the last added slot
				last = index
			}

			# If this is the first slot to be added, update the index of the first and the last slot
			if first < 0 {
				first = index
				last = index
			}

			slots[index] = slot
			size++
			return
		}
	}

	try_add(key: K, value: V) {
		if contains_key(key) return false
		add(key, value)
		return true
	}

	remove(key: K) {
		# Just return if the map is empty, this also protects from the situation where the map is not allocated yet
		if size == 0 return

		hash = key as large
		if compiles { key.hash() } { hash = key.hash() }

		attempt = 0

		# Find the slot by quadratic probing
		loop {
			index = 0
			if attempt < 10 { index = (hash + attempt * attempt) as u64 % capacity }
			else { index = (hash + attempt) as u64 % capacity }

			attempt++

			slot = slots[index]

			# Stop if we found an empty slot
			if slot.next == 0 return

			# Continue if we found a removed slot
			if slot.next == REMOVED_SLOT_MARKER continue

			# If the slot has the same key, remove it
			if slot.key == key {
				# If the slot is the first one, update the index of the first slot
				if index == first {
					first = slot.next - 1
				}

				# If the slot is the last one, update the index of the last slot
				if index == last {
					last = slot.previous - 1
				}

				# If the slot has a previous slot, connect it to the next slot
				if slot.previous > 0 {
					previous = slots[slot.previous - 1]
					previous.next = slot.next
					slots[slot.previous - 1] = previous
				}

				# If the slot has a next slot, connect it to the previous slot
				if slot.next > 0 {
					next = slots[slot.next - 1]
					next.previous = slot.previous
					slots[slot.next - 1] = next
				}

				# Update the size of the map
				size--

				# Update the number of removed slots
				# NOTE: Removed slots still slow down finding other slots and thus are taken into account in the load factor
				removed++

				# Free the slot
				slot.key = none as K
				slot.value = none as V
				slot.next = REMOVED_SLOT_MARKER
				slot.previous = REMOVED_SLOT_MARKER
				slots[index] = slot

				return
			}
		}
	}

	try_find(key: K) {
		# Just return -1 if the map is empty, this also protects from the situation where the map is not allocated yet
		if size == 0 return -1

		hash = key as large
		if compiles { key.hash() } { hash = key.hash() }

		attempt = 0

		# Find the slot by quadratic probing
		loop {
			index = 0
			if attempt < 10 { index = (hash + attempt * attempt) as u64 % capacity }
			else { index = (hash + attempt) as u64 % capacity }

			attempt++

			slot = slots[index]

			# Stop if we found an empty slot
			if slot.next == 0 return -1

			# Continue if we found a removed slot
			if slot.next == REMOVED_SLOT_MARKER continue

			# If the slot has the same key, return the value
			if slot.key == key return index
		}
	}

	contains_key(key: K) {
		return try_find(key) >= 0
	}

	get(key: K) {
		index = try_find(key)
		if index < 0 panic('Map did not contain the specified key')

		return slots[index].value
	}

	try_get(key: K) {
		index = try_find(key)
		if index < 0 return Optional<V>()

		return Optional<V>(slots[index].value)
	}

	set(key: K, value: V) {
		add(key, value)
	}

	iterator() {
		return MapIterator<K, V>(slots, first)
	}

	# Summary: Returns the keys associated with the values in this map as a list
	get_keys() {
		result = List<K>(size, false)
		index = first

		loop (index >= 0) {
			slot = slots[index]
			result.add(slot.key)

			index = slot.next - 1
		}

		return result
	}

	# Summary: Returns the values associated with the keys in this map as a list
	get_values() {
		result = List<V>(size, false)
		index = first

		loop (index >= 0) {
			slot = slots[index]
			result.add(slot.value)

			index = slot.next - 1
		}

		return result
	}

	clear() {
		if slots !== none {
			deallocate(slots)
		}

		first = -1
		last = -1
		slots = none as MapSlot<K, V>*
		capacity = 1
		size = 0
		removed = 0
	}

	# Summary: Converts the key-value pairs of this map into a list
	map<U>(mapper: (KeyValuePair<K, V>) -> U) {
		result = List<U>(items.size, false)
		index = first

		loop (index >= 0) {
			slot = slots[index]
			pair = (slots + index * sizeof(MapSlot<K, V>)) as KeyValuePair<K, V>
			result.add(mapper(pair))

			index = slot.next - 1
		}

		return result
	}
}