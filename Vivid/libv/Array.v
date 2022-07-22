export Array<T> {
	readonly data: T*
	readonly size: large

	init() {
		data = none as T*
		size = 0
	}

	# Summary: Creates an empty array with the specified size
	init(size: large) {
		require(size >= 0, 'Array size can not be negative')
		
		this.data = allocate(size * sizeof(T))
		this.size = size
	}

	# Summary: Creates an array from the specified data and size
	init(data: T*, size: large) {
		require(data != none, 'Invalid array data')
		require(size >= 0, 'Array size can not be negative')

		this.data = allocate(size * sizeof(T))
		this.size = size
		copy<T>(this.data, data, size)
	}

	# Summary: Assigns the specified value to the specified index
	set(i: large, value: T) {
		require(i >= 0 and i < size, 'Index out of bounds')

		data[i] = value
	}

	# Summary: Returns the element at the specified index
	get(i: large) {
		require(i >= 0 and i < size, 'Index out of bounds')

		return data[i]
	}

	# Summary: Returns the index of the specified element in the array. Returns -1 if the element is not found.
	index_of(value: T) {
		loop (i = 0, i < size, i++) {
			if data[i] == value return i
		}

		return -1
	}

	# Summary: Returns the elements between the specified range as an array.
	slice(start: large, end: large) {
		require(start >= 0 and start <= size, 'Invalid start index')
		require(end >= 0 and end <= size, 'Invalid end index')
		require(start <= end, 'Start index can not be greater than end index')

		return Array<T>(data + start * sizeof(T), end - start)
	}

	# Summary: Returns an iterator that can be used to inspect this array
	iterator() {
		return SequentialIterator<T>(data, size)
	}
	
	deinit() {
		if data != none deallocate(data)
	}
}