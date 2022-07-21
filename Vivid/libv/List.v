export List<T> {
	private capacity: large

	readonly data: link<T>
	readonly size: large

	private static allocate_elements(size: large) {
		if size > 0 return allocate(size * sizeof(T)) as link<T>
		return none as link<T>
	}

	# Summary: Creates a list with the specified initial size
	init(size: large, fill: bool) {
		require(size >= 0, 'Size of a list can not be negative')

		# Fill the list if requested
		if fill {
			this.size = size
		}
		else {
			this.size = 0
		}

		this.data = allocate_elements(size)
		this.capacity = size
	}

	# Summary: Creates a list with the specified initial size
	init(data: link<T>, size: large) {
		require(data != none, 'Invalid list data')
		require(size >= 0, 'Size of a list can not be negative')

		this.data = allocate_elements(size)
		this.size = size
		this.capacity = size

		copy<T>(this.data, data, size)
	}

	# Summary: Creates a list with the same elements as the specified list
	init(other: List<T>) {
		require(other != none, 'Invalid list')

		size: large = other.size

		this.data = allocate_elements(size)
		this.size = size
		this.capacity = size

		# Copy the other list into this one
		if size > 0 {
			copy<T>(this.data, other.data, size)
		}
	}

	# Summary: Creates an empty list
	init() {}

	# Summary: Grows the list to the specified size
	grow(to: large) {
		if to <= 0 { to = 1 }

		memory = allocate(to * sizeof(T))
		copy(data, size * sizeof(T), memory)

		# Free the old data
		if data !== none {
			deallocate(data)
		}

		data = memory
		capacity = to
	}

	# Summary: Adds the specified element to this list
	add(element: T) {
		if size == capacity grow(capacity * 2)

		data[size] = element
		size += 1
	}

	# Summary: Adds all the elements contained in the specified list
	add_all(other: List<T>) {
		count = other.size

		# Ensure there is enough space left
		if capacity - size < count {
			# Double the size needed for the result in order to prepare for more elements in the future
			grow((capacity + count) * 2)
		}

		# Copy the new elements to the memory after the already existing elements
		copy<T>(data + size * sizeof(T), other.data, count)
		size += count
	}

	# Summary: Puts the specified element at the specified index without removing other elements
	insert(at: large, element: T) {
		require(at >= 0 and at <= size, 'Invalid insertion index')

		if size >= capacity {
			grow(capacity * 2)
		}

		count = size - at

		if count > 0 {
			start = data + at * sizeof(T)
			move(start, start + sizeof(T), count * sizeof(T))
		}

		data[at] = element
		size++
	}

	# Summary: Puts the specified range at the specified index without removing other elements
	insert_all(at: large, other: List<T>) {
		require(other != none, 'Invalid list to insert')
		require(at >= 0 and at <= size, 'Invalid insertion index')

		count = other.size
		if count == 0 return

		# Ensure there is enough space left
		if capacity - size < count {
			# Double the size needed for the result in order to prepare for more elements in the future
			grow((capacity + count) * 2)
		}

		# Determine the address where the new elements should be inserted
		start = data + at * sizeof(T)

		# Determine how many elements must be slid to the right
		slide = size - at
		if slide > 0 move(start, start + count * sizeof(T), slide * sizeof(T))

		copy<T>(start, other.data, count)
		size += count
	}

	# Summary: Removes the element which is located at the specified index
	remove_at(at: large) {
		require(at >= 0 and at < size, 'Invalid removal index')

		# Reset the element at the specified index
		data[at] = 0

		offset = (at + 1) * sizeof(T)
		bytes = (size - at - 1) * sizeof(T)

		if bytes > 0 {
			destination = data + at * sizeof(T)
			move(data, offset, destination, bytes)
		}

		size--
	}

	# Summary: Tries to remove the first element which is equal to the specified value
	remove(element: T) {
		size: large = this.size

		loop (i = 0, i < size, i++) {
			if data[i] == element {
				remove_at(i)
				return true
			}
		}

		return false
	}

	# Summary: Removes the specified range from this list
	remove_all(start: large, end: large) {
		require(start >= 0 and start <= end, 'Invalid removal start index')
		require(end >= 0 and end <= size, 'Invalid removal end index')

		count = end - start
		if count == 0 return

		# Reset the elements at the specified range
		zero<T>(data + start * sizeof(T), count)

		slide = size - end
		move(data + end * sizeof(T), data + start * sizeof(T), slide * sizeof(T))

		size -= count
	}

	# Summary: Takes the value of the first element and removes it from the beginning of the list
	pop_or(fallback: T) {
		if size == 0 return fallback
		first = data[0]

		# Move all elements left by one
		loop (i = 1, i < size, i++) {
			data[i - 1] = data[i] 
		}

		data[--size] = fallback
		return first
	}

	# Summary: Returns the index, which contains the specified element. If it is not found, this function returns -1.
	index_of(element: T) {
		loop (i = 0, i < size, i++) {
			if data[i] == element return i
		}

		return -1
	}

	# Summary: Returns the index of the last occurrence of the specified element. If the specified element is not in the list, this function returns -1.
	last_index_of(element: T) {
		loop (i = size - 1, i >= 0, i--) {
			if data[i] == element return i
		}

		return -1
	}

	# Summary: Returns whether the list contains the specified element
	contains(element: T) {
		loop (i = 0, i < size, i++) {
			if data[i] == element return true
		}

		return false
	}

	# Summary: Creates a new list, which contains the specified range of elements
	slice(start: large, end: large) {
		require(start >= 0 and start <= end, 'Invalid slice start index')
		require(end >= 0 and end <= this.size, 'Invalid slice end index')

		# Compute the size of the result list
		size: large = end - start
		if size == 0 return List<T>()

		return List<T>(data + start * sizeof(T), size)
	}

	# Summary: Returns all the elements starting from the specified index
	slice(start: large) {
		require(start >= 0 and start <= this.size, 'Invalid slice start index')

		# Compute the size of the result list
		size: large = this.size - start
		if size == 0 return List<T>()

		return List<T>(data + start * sizeof(T), size)
	}

	# Summary: Reverses the order of the elements
	reverse() {
		size: large = this.size

		loop (i = 0, i < size / 2, i++) {
			element = data[i]
			data[i] = data[size - i - 1]
			data[size - i - 1] = element
		}

		return this
	}

	# Summary: Removes duplicated elements from this list
	distinct() {
		loop (i = 0, i < size, i++) {
			current = data[i]

			loop (j = size - 1, j > i, j--) {
				if not (data[j] == current) continue
				remove_at(j)
			}
		}

		return this
	}

	# Summary: Sets the value of the element at the specified index
	set(i: large, value: T) {
		require(i >= 0 and i < size, 'Invalid setter index')

		data[i] = value
	}

	# Summary: Returns the value at the specified index
	get(i: large) {
		require(i >= 0 and i < size, 'Invalid getter index')

		return data[i]
	}

	# Summary: Adds the specified element to this list
	assign_plus(element: T) {
		add(element)
	}

	# Summary: Removes the specified element to this list
	assign_minus(element: T) {
		remove(element)
	}

	# Summary: Adds the elements from this and the specified list to a new list
	plus(other: List<T>) {
		require(other != none, 'Invalid list to combine')

		size: large = this.size
		result = List<T>(size + other.size, true)

		# Copy this list into the result list
		if data !== none {
			copy<T>(result.data, data, size)
		}

		# Copy the other list into the result list
		if other.data !== none {
			copy<T>(result.data + size * sizeof(T), other.data, other.size)
		}

		return result
	}

	# Summary: Adds the elements from this and the specified element to a new list
	plus(element: T) {
		size: large = this.size
		result = List<T>(size + 1, true)

		# Copy this list into the result list
		if data !== none {
			copy<T>(result.data, data, size)
		}

		# Copy the specified element into the result list
		result[size] = element

		return result
	}

	# Summary: Removes all the elements from this list
	clear() {
		# Free the old data
		if data !== none {
			deallocate(data)
		}

		data = none as link<T>
		capacity = 0
		size = 0
	}

	# Summary: Returns a list of all elements which pass the specified filter
	filter(filter: (T) -> bool) {
		result = List<T>()

		loop (i = 0, i < size, i++) {
			if filter(data[i]) result.add(data[i])
		}

		return result
	}

	# Summary: Returns the first element, which passes the specified filter, otherwise the function panics
	find(filter: (T) -> bool) {
		loop (i = 0, i < size, i++) {
			element = data[i]
			if filter(element) return element
		}

		panic('No element passed the filter')
	}

	# Summary: Return the first element, which passes the specified filter, or the default value if no element passes the filter
	find_or(filter: (T) -> bool, default: T) {
		loop (i = 0, i < size, i++) {
			element = data[i]
			if filter(element) return element
		}

		return default
	}

	# Summary: Returns the index of the first element, which passes the specified filter, otherwise the function returns -1
	find_index(filter: (T) -> bool) {
		loop (i = 0, i < size, i++) {
			element = data[i]
			if filter(element) return i
		}

		return -1
	}

	# Summary: Returns the first element to produce the maximum value using the specified mapper
	find_max(mapper: (T) -> large) {
		if size == 0 panic('Can not find the maximum value of an empty list')

		max = data[0]
		max_value = mapper(max)

		loop (i = 1, i < size, i++) {
			element = data[i]
			value = mapper(element)

			if value > max_value {
				max = element
				max_value = value
			}
		}

		return max
	}

	# Summary: Returns the first element to produce the maximum value using the specified mapper
	find_max_or(mapper: (T) -> large, default: T) {
		if size == 0 return default

		max = data[0]
		max_value = mapper(max)

		loop (i = 1, i < size, i++) {
			element = data[i]
			value = mapper(element)

			if value > max_value {
				max = element
				max_value = value
			}
		}

		return max
	}

	# Summary: Converts the elements of this list into another types of elements
	map<U>(mapper: (T) -> U) {
		result = List<U>(size, true)

		loop (i = 0, i < size, i++) {
			result[i] = mapper(data[i])
		}

		return result
	}

	# Summary: Creates a new list of all the element collections returned by the mapper by adding them sequentially
	flatten<U>(mapper: (T) -> List<U>) {
		result = List<U>(size, false)

		loop (i = 0, i < size, i++) {
			collection = mapper(data[i])
			result.add_all(collection)
		}

		return result
	}

	# Summary: Sorts the elements in this list using the specified comparator
	order(comparator: (T, T) -> large) {
		if size == 0 return this

		sort<T>(data, size, comparator)
		return this
	}

	# Summary: Returns the number of elements which pass the specified filter
	count(filter: (T) -> bool) {
		count = 0

		loop (i = 0, i < size, i++) {
			if filter(data[i]) { count++ }
		}

		return count
	}
	
	# Summary: Returns whether all the elements pass the specified filter
	all(filter: (T) -> bool) {
		loop (i = 0, i < size, i++) {
			if not filter(data[i]) return false
		}

		return true
	}

	# Summary: Returns true if any of the elements pass the specified filter
	any(filter: (T) -> bool) {
		loop (i = 0, i < size, i++) {
			if filter(data[i]) return true
		}

		return false
	}

	# Summary: Returns an iterator which can be used to inspect this list
	iterator() {
		return SequentialIterator<T>(data, size)
	}

	deinit() {
		if data === none return
		zero<T>(data, size)
		deallocate(data)
	}
}