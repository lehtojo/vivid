export List<T> {
	private:
	capacity: large
	position: large

	public:
	readonly elements: link<T>

	# Summary: Creates a list with the specified initial size
	init(size: large, fill: bool) {
		require(size >= 0, 'Invalid list size')

		# Fill the list if requested
		if fill { position = size }
		else { position = 0 }

		# Set minimum size to 1
		if size <= 0 { size = 1 }

		elements = allocate(size * sizeof(T))
		capacity = size
	}

	# Summary: Creates a list with the specified initial size
	init(elements: link<T>, size: large) {
		require(elements != none, 'Invalid list data')
		require(size >= 0, 'Invalid list size')

		this.elements = allocate(size * sizeof(T))
		this.position = size
		this.capacity = size

		copy<T>(this.elements, elements, size)
	}

	# Summary: Creates a list with the same contents as the specified list
	init(other: List<T>) {
		require(other != none, 'Invalid list')

		size = other.size
		this.elements = allocate(size * sizeof(T))
		this.position = size
		this.capacity = size

		copy<T>(this.elements, other.elements, size)
	}

	# Summary: Creates an empty list
	init() {
		elements = allocate(sizeof(T))
		capacity = 1
		position = 0
	}

	# Summary: Grows the list to the specified size
	grow(to: large) {
		if to == 0 { to = 1 }

		memory = allocate(to * sizeof(T))
		copy(elements, position * sizeof(T), memory)
		deallocate(elements)

		elements = memory
		capacity = to
	}

	# Summary: Adds the specified element to this list
	add(element: T) {
		if position == capacity {
			grow(capacity * 2)
		}

		elements[position] = element
		position += 1
	}

	# Summary: Adds all the elements contained in the specified list
	add_range(other: List<T>) {
		count = other.size()

		# Ensure there is enough space left
		if capacity - position < count {
			# Double the size needed for the result in order to prepare for more elements in the future
			grow((capacity + count) * 2)
		}

		# Copy the new elements to the memory after the already existing elements
		copy<T>(elements + position * sizeof(T), other.elements, count)
		position += count
	}

	# Summary: Puts the specified element at the specified index without removing other elements
	insert(at: large, element: T) {
		require(at >= 0 and at <= position, 'Invalid insertion index')

		if position >= capacity {
			grow(capacity * 2)
		}

		count = position - at

		if count > 0 {
			start = elements + at * sizeof(T)
			move(start, start + sizeof(T), count * sizeof(T))
		}

		elements[at] = element
		position++
	}

	# Summary: Puts the specified range at the specified index without removing other elements
	insert_range(at: large, other: List<T>) {
		require(other != none, 'Invalid list to insert')
		require(at >= 0 and at <= position, 'Invalid insertion index')

		count = other.size()
		if count == 0 return

		# Ensure there is enough space left
		if capacity - position < count {
			# Double the size needed for the result in order to prepare for more elements in the future
			grow((capacity + count) * 2)
		}

		# Determine the address where the new elements should be inserted
		start = elements + at * sizeof(T)

		# Determine how many elements must be slided to the right
		slide = position - at
		if slide > 0 move(start, start + count * sizeof(T), slide * sizeof(T))

		copy<T>(start, other.elements, count)
		position += count
	}

	# Summary: Removes the element which is located at the specified index
	remove_at(at: large) {
		require(at >= 0 and at < position, 'Invalid removal index')

		# Reset the element at the specified index
		elements[at] = 0

		offset = (at + 1) * sizeof(T)
		bytes = (position - at - 1) * sizeof(T)

		if bytes > 0 {
			destination = elements + at * sizeof(T)
			move(elements, offset, destination, bytes)
		}

		position--
	}

	# Summary: Tries to remove the first element which is equal to the specified value
	remove(element: T) {
		count = size()

		loop (i = 0, i < count, i++) {
			if elements[i] == element {
				remove_at(i)
				stop
			}
		}
	}

	# Summary: Removes the specified range from this list
	remove_range(start: large, end: large) {
		require(start >= 0 and start <= end, 'Invalid removal start index')
		require(end >= 0 and end < position, 'Invalid removal end index')

		count = end - start
		if count == 0 return

		# Reset the elements at the specified range
		zero<T>(elements + start * sizeof(T), count)

		slide = position - end
		move(elements + end * sizeof(T), elements + start * sizeof(T), slide * sizeof(T))

		position -= count
	}

	# Summary: Takes the value of the first element and removes it from the begining of the list
	pop_or(fallback: T) {
		if position == 0 => fallback
		first = elements[0]

		# Move all elements left by one
		loop (i = 1, i < position, i++) {
			elements[i - 1] = elements[i] 
		}

		elements[--position] = fallback
		=> first
	}

	# Summary: Returns the index, which contains the specified element. If it is not found, this function returns -1.
	index_of(element: T) {
		loop (i = 0, i < position, i++) {
			if elements[i] == element => i
		}

		=> -1
	}

	# Summary: Returns the index of the last occurance of the specified element. If the specified element is not in the list, this function returns -1.
	last_index_of(element: T) {
		loop (i = position - 1, i >= 0, i--) {
			if elements[i] == element => i
		}

		=> -1
	}

	# Summary: Returns whether the list contains the specified element
	contains(element: T) {
		loop (i = 0, i < position, i++) {
			if elements[i] == element => true
		}

		=> false
	}

	# Summary: Creates a new list, which contains the specified range of elements
	slice(start: large, end: large) {
		require(start >= 0 and start <= end, 'Invalid slice start index')
		require(end >= 0 and end <= position, 'Invalid slice end index')

		=> List<T>(elements + start * sizeof(T), end - start)
	}

	# Summary: Returns all the elements starting from the specified index
	slice(start: large) {
		require(start >= 0 and start <= position, 'Invalid slice start index')

		=> List<T>(elements + start * sizeof(T), position - start)
	}

	# Summary: Reverses the order of the elements
	reverse() {
		position: large = this.position

		loop (i = 0, i < position / 2, i++) {
			element = elements[i]
			elements[i] = elements[position - i - 1]
			elements[position - i - 1] = element
		}

		=> this
	}

	# Summary: Removes duplicated elements from this list
	distinct() {
		loop (i = 0, i < position, i++) {
			current = elements[i]

			loop (j = position - 1, j > i, j--) {
				if not (elements[j] == current) continue
				remove_at(j)
			}
		}

		=> this
	}

	# Summary: Sets the value of the element at the specified index
	set(i: large, value: T) {
		require(i >= 0 and i < position, 'Invalid setter index')

		elements[i] = value
	}

	# Summary: Returns the value at the specified index
	get(i: large) {
		require(i >= 0 and i < position, 'Invalid getter index')

		=> elements[i]
	}

	# Summary: Adds the specified element to this list
	assign_plus(element: T) {
		add(element)
	}

	# Summary: Returns the size of this list
	size() {
		=> position
	}

	# Summary: Returns an iterator which can be used to inspect this list
	iterator() {
		=> MemoryIterator<T>(elements, position)
	}

	# Summary: Creates an array which contains the same elements as this list
	to_array() {
		=> Array<T>(elements, position)
	}

	# Summary: Removes all the elements from this list
	clear() {
		position = 0
	}

	# Summary: Returns a list of all elements which pass the specified filter
	filter(filter: (T) -> bool) {
		result = List<T>()

		loop (i = 0, i < position, i++) {
			if filter(elements[i]) result.add(elements[i])
		}

		=> result
	}

	# Summary: Returns the first element, which passes the specified filter, otherwise the function panics
	find(filter: (T) -> bool) {
		loop (i = 0, i < position, i++) {
			element = elements[i]
			if filter(element) => element
		}

		panic('No element passed the filter')
	}

	# Summary: Return the first element, which passes the specified filter, or the default value if no element passes the filter
	find_or(filter: (T) -> bool, default: T) {
		loop (i = 0, i < position, i++) {
			element = elements[i]
			if filter(element) => element
		}

		=> default
	}

	# Summary: Returns the first element to produce the maximum value using the specified mapper
	find_max(mapper: (T) -> large) {
		if position == 0 panic('Can not find the maximum value of an empty list')

		max = elements[0]
		max_value = mapper(max)

		loop (i = 1, i < position, i++) {
			element = elements[i]
			value = mapper(element)

			if value > max_value {
				max = element
				max_value = value
			}
		}

		=> max
	}

	# Summary: Returns the first element to produce the maximum value using the specified mapper
	find_max_or(mapper: (T) -> large, default: T) {
		if position == 0 => default

		max = elements[0]
		max_value = mapper(max)

		loop (i = 1, i < position, i++) {
			element = elements[i]
			value = mapper(element)

			if value > max_value {
				max = element
				max_value = value
			}
		}

		=> max
	}

	# Summary: Converts the elements of this list into another types of elements
	map<U>(mapper: (T) -> U) {
		result = List<U>(position, true)

		loop (i = 0, i < position, i++) {
			result[i] = mapper(elements[i])
		}

		=> result
	}

	# Summary: Creates a new list of all the element collections returned by the mapper by adding them sequentially
	flatten<U>(mapper: (T) -> List<U>) {
		result = List<U>(position, false)

		loop (i = 0, i < position, i++) {
			collection = mapper(elements[i])
			result.add_range(collection)
		}

		=> result
	}

	# Summary: Sorts the elements in this list using the specified comparator
	order(comparator: (T, T) -> large) {
		sort<T>(elements, position, comparator)
		=> this
	}

	# Summary: Returns the number of elements which pass the specified filter
	count(filter: (T) -> bool) {
		count = 0

		loop (i = 0, i < position, i++) {
			if filter(elements[i]) count++
		}

		=> count
	}
	
	# Summary: Returns whether all the elements pass the specified filter
	all(filter: (T) -> bool) {
		loop (i = 0, i < position, i++) {
			if not filter(elements[i]) => false
		}

		=> true
	}

	# Summary: Returns true if any of the elements pass the specified filter
	any(filter: (T) -> bool) {
		loop (i = 0, i < position, i++) {
			if filter(elements[i]) => true
		}

		=> false
	}

	# TODO: Decrement operator overload for taking out elements

	deinit() {
		zero<T>(elements, position)
		deallocate(elements)
	}
}