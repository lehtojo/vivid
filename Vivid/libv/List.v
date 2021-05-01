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

	# Summary: Creates a list with the specified initial size
	init(count: large) {
		if count <= 0 {
			count = 1
		}

		elements = allocate(count * sizeof(T))
		zero(elements, count * sizeof(T))

		capacity = count
		position = 0
	}

	# Summary: Creates a list with the specified initial size
	init(elements: link<T>, size: large) {
		this.elements = allocate(size * sizeof(T))
		this.position = size
		this.capacity = size
		
		copy(elements, size * sizeof(T), this.elements)
	}

	# Summary: Creates an empty list
	init() {
		elements = allocate(sizeof(T))
		elements[0] = 0 as T

		capacity = 1
		position = 0
	}

	# Summary: Grows the list by doubling its size
	private grow() {
		memory = allocate(capacity * 2 * sizeof(T))
		zero(memory + capacity * sizeof(T), capacity * sizeof(T))
		copy(elements, capacity * sizeof(T), memory)
		deallocate(elements)

		elements = memory
		capacity = capacity * 2
	}

	# Summary: Adds the specified element to this list
	add(element: T) {
		if position == capacity {
			grow()
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
			capacity = (capacity + count) * 2

			# Allocate a chunk of memory which has the new capacity
			memory = allocate(capacity * sizeof(T))
			zero(memory, capacity * sizeof(T))

			# Copy the already existing elements to the memory chunk and deallocate the old memory chunk
			copy(elements, position * sizeof(T), memory)
			deallocate(elements)

			elements = memory
		}

		# Copy the new elements to the memory after the already existing elements
		copy(other.elements, count * sizeof(T), elements as link + position * sizeof(T))
		position += count
	}

	# Summary: Puts the specified element at the specified index without removing other elements
	insert(at: large, element: T) {
		if position >= capacity {
			grow()
		}

		count = position - at

		if count > 0 {
			start = elements + at * sizeof(T)
			move(start, start + sizeof(T), count)
		}

		elements[at] = element
		position++
	}

	# Summary: Removes the element which is located at the specified index
	remove_at(at: large) {
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

	# Summary: Takes the value of the first element and removes it from the begining of the list
	take_first() {
		first = elements[0]

		# Move all elements left by one
		loop (i = 1, i < position, i++) {
			elements[i - 1] = elements[i] 
		}

		elements[--position] = none as T
		=> first
	}
	
	# Summary: Sets the value of the element at the specified index
	set(i: large, value: T) {
		elements[i] = value
	}

	# Summary: Returns the value at the specified index
	get(i: large) {
		=> elements[i]
	}
	
	# Summary: Adds the specified element to this list
	assign_plus(element: T) {
		add(element)
	}

	# Summary: Returns the size of this list
	size() => position

	# Summary: Returns an iterator which can be used to inspect this list
	iterator() {
		=> MemoryIterator<T>(elements, position)
	}

	# Summary: Creates an array which contains the same elements as this list
	to_array() {
		=> Array<T>(elements, position)
	}
}