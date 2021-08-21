KeyValuePair<K, V> {
	key: K
	value: V

	init(key: K, value: V) {
		this.key = key
		this.value = value
	}
}

MAX_SLOT_OFFSET = 10
MAX_LEVEL_SIZE = 1024

Map<K, V> {
	private:
	values: link<V>
	keys: link<K>
	states: link<bool>
	items: List<KeyValuePair<K, V>> = List<KeyValuePair<K, V>>()
	ground: tiny
	levels: normal = 1

	public:
	init(ground: tiny) {
		count = 1 <| ground
		values_size = count * sizeof(V)
		keys_size = count * sizeof(K)

		values: link = allocate(values_size)
		keys: link = allocate(keys_size)
		states: link = allocate(count)

		zero(values, values_size)
		zero(keys, keys_size)
		zero(states, count)

		this.ground = ground
		this.values = values
		this.keys = keys
		this.states = states
	}

	init() {
		count = 1 <| 6
		values_size = count * sizeof(V)
		keys_size = count * sizeof(K)

		values: link = allocate(values_size)
		keys: link = allocate(keys_size)
		states: link = allocate(count)

		zero(values, values_size)
		zero(keys, keys_size)
		zero(states, count)

		this.ground = 6
		this.values = values
		this.keys = keys
		this.states = states
	}

	grow() {
		count = 0

		loop (i = ground, i < ground + levels, i++) {
			count += min(1 <| i, MAX_LEVEL_SIZE)
		}

		values_size = count * sizeof(V)
		keys_size = count * sizeof(K)

		extended_values = allocate(values_size)
		extended_keys = allocate(keys_size)
		extended_states = allocate(count)

		zero(extended_values, values_size)
		zero(extended_keys, keys_size)
		zero(extended_states, count)

		previous_count = count - min(1 <| [ground + levels - 1], MAX_LEVEL_SIZE)

		copy(values, previous_count * sizeof(V), extended_values)
		copy(keys, previous_count * sizeof(K), extended_keys)
		copy(states, previous_count, extended_states)

		deallocate(values)
		deallocate(keys)
		deallocate(states)

		values = extended_values
		keys = extended_keys
		states = extended_states
	}

	force_add(key: K, value: V) {
		items.add(KeyValuePair<K, V>(key, value))

		hash = key as large
		if compiles { key.hash() } { hash = key.hash() }

		position = 0
		location = 0
		size = 1

		loop (i = ground, i < ground + levels, i++) {
			size = min(1 <| i, MAX_LEVEL_SIZE)

			location = hash % size
			if location < 0 { location += size }

			start = position + location
			n = min(MAX_SLOT_OFFSET, position + size - location)

			loop (j = 0, j < n, j++) {
				offset = start + j
				if states[offset] continue
				states[offset] = true
				keys[offset] = key
				values[offset] = value
				return
			}

			position += size
		}

		levels++
		grow()

		size = min(1 <| (ground + levels - 1), MAX_LEVEL_SIZE)
		location = hash % size
		if location < 0 { location += size }

		position += location
		states[position] = true
		keys[position] = key
		values[position] = value
	}

	add(key: K, value: V) {
		if contains_key(key) require(false, 'Map already contains the specified key')
		force_add(key, value)
	}

	try_add(key: K, value: V) {
		if contains_key(key) => false
		force_add(key, value)
		=> true
	}

	set(key: K, value: V) {
		location = try_find(key)

		if location >= 0 {
			keys[location] = key
			values[location] = value
			return
		}

		force_add(key, value)
	}

	try_find(key: K) {
		hash = key as large
		if compiles { key.hash() } { hash = key.hash() }

		position = 0
		size = 1

		loop (i = ground, i < ground + levels, i++) {
			size = min(1 <| i, MAX_LEVEL_SIZE)

			location = hash % size
			if location < 0 { location += size }
			
			start = position + location
			n = min(MAX_SLOT_OFFSET, position + size - start)

			loop (j = 0, j < n, j++) {
				offset = start + j
				if states[offset] and keys[offset] == key => offset
			}

			position += size
		}

		=> -1
	}

	contains_key(key: K) {
		=> try_find(key) >= 0
	}

	try_get(key: K) {
		location = try_find(key)
		if location >= 0 => Optional<V>(values[location])
		=> Optional<V>()
	}

	get(key: K) {
		location = try_find(key)
		if location >= 0 => values[location]
		require(false, 'Map did not contain the specified key')
	}

	remove(key: K) {
		location = try_find(key)
		if location < 0 => false
		
		states[location] = false

		loop (i = 0, i < items.size, i++) {
			if not (items[i].key == key) continue
			items.remove_at(i)
			stop
		}

		=> true
	}

	size() {
		=> items.size
	}

	iterator() {
		=> items.iterator()
	}
}