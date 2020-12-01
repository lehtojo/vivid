List<T> {

	private:
	elements: link
	capacity: num
	position: num

	public:

	init(count: num) {
		if count == 0 {
			count = 1
		}

		elements = allocate(count * T.size)
		capacity = count
		position = 0
	}

	init() {
		elements = allocate(T.size)
		capacity = 1
		position = 0
	}

	private grow() {
		memory = allocate(capacity * 2 * T.size)
		copy(elements, capacity * T.size, memory)
		#deallocate(elements, capacity * T.size)

		elements = memory
		capacity = capacity * 2
	}

	add(element: T) {
		if position == capacity {
			grow()
		}

		elements[position * T.size] as T = element
		position += 1
	}

	remove(at: num) {
		offset = (at + 1) * T.size
		bytes = (position - at - 1) * T.size

		if bytes > 0 {
			destination = elements + at * T.size

			move(elements, offset, destination, bytes)
		}

		position--
	}

	take() {
		position -= 1
		=> elements[position * T.size] as T
	}
	
	set(i: num, value: T) {
		elements[i * T.size] as T = value
	}

	get(i: num) {
		=> elements[i * T.size] as T
	}
	
	assign_plus(element: T) {
		add(element)
	}

	size() => position
}