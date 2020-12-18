List<T> {

	private:
	elements: link<T>
	capacity: large
	position: large

	public:

	init(count: large) {
		if count == 0 {
			count = 1
		}

		elements = allocate(count * sizeof(T))
		capacity = count
		position = 0
	}

	init() {
		elements = allocate(sizeof(T))
		capacity = 1
		position = 0
	}

	private grow() {
		memory = allocate(capacity * 2 * sizeof(T))
		copy(elements, capacity * sizeof(T), memory)
		#deallocate(elements, capacity * sizeof(T))

		elements = memory
		capacity = capacity * 2
	}

	add(element: T) {
		if position == capacity {
			grow()
		}

		elements[position] = element
		position += 1
	}

	remove(at: large) {
		offset = (at + 1) * sizeof(T)
		bytes = (position - at - 1) * sizeof(T)

		if bytes > 0 {
			destination = elements + at * sizeof(T)

			move(elements, offset, destination, bytes)
		}

		position--
	}

	take() {
		position -= 1
		=> elements[position]
	}
	
	set(i: large, value: T) {
		elements[i] = value
	}

	get(i: large) {
		=> elements[i]
	}
	
	assign_plus(element: T) {
		add(element)
	}

	size() => position
}