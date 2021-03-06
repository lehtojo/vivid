MemoryIterator<T> {
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
		zero(elements, count * sizeof(T))

		capacity = count
		position = 0
	}

	init() {
		elements = allocate(sizeof(T))
		elements[0] = 0 as T

		capacity = 1
		position = 0
	}

	private grow() {
		memory = allocate(capacity * 2 * sizeof(T))
		zero(memory + capacity * sizeof(T), capacity * sizeof(T))
		copy(elements, capacity * sizeof(T), memory)
		deallocate(elements)

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

	remove_at(at: large) {
		offset = (at + 1) * sizeof(T)
		bytes = (position - at - 1) * sizeof(T)

		if bytes > 0 {
			destination = elements + at * sizeof(T)

			move(elements, offset, destination, bytes)
		}

		position--
	}

	remove(element: T) {
		count = size()

		loop (i = 0, i < count, i++) {
			if elements[i] == element {
				remove_at(i)
				stop
			}
		}
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

	iterator() {
		=> MemoryIterator<T>(elements, position)
	}
}