REQUIREMENT_EXIT_CODE = 1

export MemoryIterator<T> {
	elements: link<T>
	position: normal
	count: normal

	init(elements: link<T>, count: large) {
		this.elements = elements
		this.position = -1
		this.count = count
	}

	value() => elements[position]

	next() {
		=> ++position < count
	}

	reset() {
		position = -1
	}
}

export Array<T> {
	public readonly data: link<T>
	count: large

	init() {
		data = none as link<T>
		count = 0
	}
	
	init(count: large) {
		require(count >= 0, 'Invalid array size')
		
		this.data = allocate(count * sizeof(T))
		this.count = count
	}

	# Summary: Creates an array from the specified data and size
	init(data: link<T>, count: large) {
		require(data != none, 'Invalid array data')
		require(count >= 0, 'Invalid array size')

		this.data = allocate(count * sizeof(T))
		this.count = count
		copy<T>(this.data, data, count)
	}
	
	set(i: large, value: T) {
		require(i >= 0 and i < count, 'Index out of bounds')
		
		data[i] = value
	}
	
	get(i: large) {
		require(i >= 0 and i < count, 'Index out of bounds')

		=> data[i]
	}

	# Summary: Returns the index of the specified element in the array. Returns -1 if the element is not found.
	index_of(value: T) {
		loop (i = 0, i < count, i++) {
			if data[i] == value => i
		}
		=> -1
	}

	# Summary: Returns the elements between the specified range as an array.
	slice(start: large, end: large) {
		require(start >= 0 and start < count, 'Invalid start index')
		require(end >= 0 and end <= count, 'Invalid end index')
		require(start <= end, 'Invalid slice')

		=> Array<T>(data + start * sizeof(T), end - start)
	}

	# Summary: Returns an iterator which can be used to inspect this array
	iterator() {
		=> MemoryIterator<T>(data, count)
	}
	
	deinit() {
		if data != none deallocate(data)
	}

	# Summary: Creates a list which contains the same elements as this array
	to_list() {
		=> List<T>(data, count)
	}
}

# Summary: Finds the slices from this string which are separated by the specified character and returns them as an array
String.split(character: char) {
	c = 1

	# Count the number of splits
	loop (i = 0, i < length, i++) {
		if text[i] != character continue
		c++
	}

	# Reserve a result array for the slices 
	slices = Array<String>(c)

	c = 0
	i = 0
	p = 0

	loop (i < length) {
		if text[i] != character {
			i++
			continue
		}
		
		slices[c++] = String(text + p, i - p)
		p = ++i
	}

	slices[c] = String(text + p, i - p)
	=> slices
}

export Sheet<T> {
	public readonly data: link<T>
	width: large
	height: large
	
	init(width: large, height: large) {
		require(width >= 0 and height >= 0, 'Invalid sheet size')
		
		size = width * height * sizeof(T)

		this.data = allocate(size)
		zero(this.data, size)

		this.width = width
		this.height = height
	}
	
	set(x: large, y: large, value: T) {
		require(x >= 0 and x < width and y >= 0 and y <= height, 'Index out of bounds')
		data[y * width + x] = value
	}
	
	get(x: large, y: large) {
		require(x >= 0 and x < width and y >= 0 and y <= height, 'Index out of bounds')
		=> data[y * width + x]
	}
	
	deinit() {
		deallocate(data)
	}
}

export Box<T> {
	public readonly data: link<T>
	width: large
	height: large
	depth: large
		
	init(width: large, height: large, depth: large) {
		require(width >= 0 and height >= 0 and depth >= 0, 'Invalid box size')
		
		size = width * height * depth * sizeof(T)

		this.data = allocate(size)
		zero(this.data, size)

		this.width = width
		this.height = height
		this.depth = depth
	}
		
	set(x: large, y: large, z: large, value: T) {
		require(x >= 0 and x < width and y >= 0 and y <= height and z >= 0 and z <= depth, 'Index out of bounds')
		data[z * width * height + y * width + x] = value
	}
		
	get(x: large, y: large, z: large) {
		require(x >= 0 and x < width and y >= 0 and y <= height and z >= 0 and z <= depth, 'Index out of bounds')
		=> data[z * width * height + y * width + x]
	}
		
	deinit() {
		deallocate(data)
	}
}


# Summary: Ensures the specified condition is true, otherwise this function exits the program and informs that a requirement was not met
export require(result: bool) {
	if result == false {
		println('Requirement failed')
		application.exit(REQUIREMENT_EXIT_CODE)
	}
}

# Summary: Ensures the specified condition is true, otherwise this function exits the program and informs the user with the specified message
export require(result: bool, message: link) {
	if result == false {
		println(message)
		application.exit(REQUIREMENT_EXIT_CODE)
	}
}