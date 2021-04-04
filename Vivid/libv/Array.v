REQUIREMENT_EXIT_CODE = 1

export require(result: bool) {
	if result == false {
		println('Requirement failed')
		exit(REQUIREMENT_EXIT_CODE)
	}
}

export require(result: bool, message: link) {
	if result == false {
		println(message)
		exit(REQUIREMENT_EXIT_CODE)
	}
}

Array<T> {
	private data: link<T>
	count: large
	
	init(count: large) {
		require(count >= 0, 'Invalid array size')
		
		size = count * sizeof(T)

		this.data = allocate(size)
		zero(this.data, size)
		
		this.data = allocate(count * sizeof(T))
		this.count = count
	}
	
	set(i: large, value: T) {
		require(i >= 0 and i < count, 'Index out of bounds')
		
		data[i] = value
	}
	
	get(i: large) {
		require(i >= 0 and i < count, 'Index out of bounds')

		=> data[i]
	}
	
	deinit() {
		deallocate(data)
	}
}

Sheet<T> {
	private data: link<T>
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

Box<T> {
	private data: link<T>
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