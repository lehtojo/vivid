require(result: bool) {
    if !result {
        println('Requirement failed')
        exit(1)
    }
}

require(result: bool, message: link) {
    if !result {
        println(message)
        exit(1)
    }
}

Array { T } {
    private data: link
    count: num
    
    init(count: num) {
        require(count >= 0, 'Tried to create a standard array but its size was a negative value')
        
        this.data = allocate(count * T.size)
        this.count = count
    }
    
    set(i: num, value: T) {
        require(i >= 0 and i < count)
        
        data[i * T.size] as T = value
    }
    
    get(i: num) {
        require(i >= 0 and i < count)
        
        => data[i * T.size] as T
    }
    
    deinit() {
        deallocate(data, count * T.size)
    }
}

Sheet { T } {
    private data: link
    width: num
    height: num
    
    init(width: num, height: num) {
        require(width >= 0 and height >= 0, 'Tried to create a sheet but its width or height was a negative value')
        
        this.data = allocate(width * height * T.size)
        this.width = width
        this.height = height
    }
    
    set(x: num, y: num, value: T) {
        require(x >= 0 and x < width and y >= 0 and y <= height)
        data[(y * width + x) * T.size] as T = value
    }
    
    get(x: num, y: num) {
        require(x >= 0 and x < width and y >= 0 and y <= height)
        => data[(y * width + x) * T.size] as T
    }
    
    deinit() {
        deallocate(width * height * T.size)
    }
}

Box { T } {
    private data: link
    width: num
    height: num
    depth: num
        
    init(width: num, height: num, depth: num) {
        require(width >= 0 and height >= 0 and depth >= 0, 'Tried to create a box but its width, height or depth was a negative value')
            
        this.data = allocate(width * height * depth * T.size)
        this.width = width
        this.height = height
        this.depth = depth
    }
        
    set(x: num, y: num, z: num, value: T) {
        require(x >= 0 and x < width and y >= 0 and y <= height and z >= 0 and z <= depth)
        data[(z * width * height + y * width + x) * T.size] as T = value
    }
        
    get(x: num, y: num, z: num) {
        require(x >= 0 and x < width and y >= 0 and y <= height and z >= 0 and z <= depth)
        => data[(z * width * height + y * width + x) * T.size] as T
    }
        
    deinit() {
        deallocate(width * height * depth * T.size)
    }
}