ArrayIterator<T> {
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

Array<T> {
	private data: link<T>
	count: large
	
	init(count: large) {
		this.data = allocate(count * sizeof(T))
		this.count = count
	}
	
	set(i: large, value: T) {
		data[i] = value
	}
	
	get(i: large) {
		=> data[i]
	}

	iterator() => ArrayIterator<T>(data, count)
	
	deinit() {
		deallocate(data)
	}
}

Object {
	public:
	value: decimal
	flag: bool = false
	
	value() {
		flag = true
		=> value
	}
}

export iteration_1(array: Array<large>, destination: link<large>) {
	loop i in array {
		destination[0] = i
		destination += sizeof(large)
	}
}

export iteration_2(destination: link<large>) {
	loop i in -10..10 {
		destination[0] = i * i
		destination += sizeof(large)
	}
}

export iteration_3(range: Range, destination: link<large>) {
	loop i in range {
		destination[0] = 2 * i
		destination += sizeof(large)
	}
}

export iteration_4(objects: Array<Object>) {
	loop i in objects {
		if i.value() > -10.0 and i.value() < 10.0 {
			stop
		}
	}
}

export iteration_5(objects: Array<Object>) {
	loop i in objects {
		if i.value() < -12.34 or i.value() > 12.34 {
			continue
		}

		stop
	}
}

export range_1() {
	=> 1..10
}

export range_2() {
	=> -5e2..10e10
}

export range_3(a: large, b: large) {
	=> a..b
}

export range_4(a: large, b: large) {
	=> a * a .. b * b
}

init() {
	a = Object()
	a.value = 12.345
	a.flag = false

	b = Object()
	b.value = -12.34
	b.flag = false

	c = Object()
	c.value = 101
	c.flag = false

	array = Array<Object>(3)
	array[0] = a
	array[1] = b
	array[2] = c

	#iteration_5(array)
	=> 1
}