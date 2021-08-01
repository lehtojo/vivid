List<T> {
	private:
	capacity: large
	position: large

	public:
	readonly elements: link<T>

	# Summary: Creates a list with the specified initial size
	init(size: large, fill: bool) {
		if fill { position = size }
		else { position = 0 }

		if size <= 0 { size = 1 }

		elements = allocate(size * sizeof(T))
		zero(elements, size * sizeof(T))

		capacity = size
	}

	# Summary: Creates a list with the specified initial size
	init(elements: link<T>, size: large) {
		this.elements = allocate(size * sizeof(T))
		this.position = size
		this.capacity = size
		
		copy(elements, size * sizeof(T), this.elements)
	}

	# Summary: Creates a list with the same contents as the specified list
	init(other: List<T>) {
		size = other.size
		this.elements = allocate(size * sizeof(T))
		this.position = size
		this.capacity = size
		
		copy(other.elements, size * sizeof(T), this.elements)
	}

	# Summary: Creates an empty list
	init() {
		elements = allocate(sizeof(T))
		elements[0] = 0 as T

		capacity = 1
		position = 0
	}

	# Summary: Grows the list to the specified size
	private grow(to: large) {
		if to == 0 { to = 1 }
		memory = allocate(to * sizeof(T))
		zero(memory + position * sizeof(T), (to - position) * sizeof(T))
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
			grow([capacity + count] * 2)
		}

		# Copy the new elements to the memory after the already existing elements
		copy(other.elements, count * sizeof(T), elements as link + position * sizeof(T))
		position += count
	}

	# Summary: Puts the specified element at the specified index without removing other elements
	insert(at: large, element: T) {
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
		count = other.size()
		if count == 0 return

		# Ensure there is enough space left
		if capacity - position < count {
			# Double the size needed for the result in order to prepare for more elements in the future
			grow([capacity + count] * 2)
		}

		# Determine the address where the new elements should be inserted
		start = elements + at * sizeof(T)

		# Determine how many elements must be slided to the right
		slide = position - at
		if slide > 0 move(start, start + count * sizeof(T), slide * sizeof(T))

		copy(other.elements, count * sizeof(T), start)
		position += count
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

	# Summary: Removes the specified range from this list
	remove_range(start: large, end: large) {
		count = end - start
		if count == 0 return
		slide = position - end
		move(elements + end * sizeof(T), elements + start * sizeof(T), slide * sizeof(T))
		position -= count
	}

	# Summary: Takes the value of the first element and removes it from the begining of the list
	take_first() {
		if position == 0 => none as T
		first = elements[0]

		# Move all elements left by one
		loop (i = 1, i < position, i++) {
			elements[i - 1] = elements[i] 
		}

		elements[--position] = none as T
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
		=> List<T>(elements + start * sizeof(T), end - start)
	}

	# Summary: Reverses the order of the elements
	reverse() {
		position: large = this.position

		loop (i = 0, i < position / 2, i++) {
			element = elements[i]
			elements[i] = elements[position - i - 1]
			elements[position - i - 1] = element
		}
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

	# Summary: Removes all the elements from this list
	clear() {
		position = 0
	}

	# TODO: Decrement operator overload for taking out elements
	# TODO: Destruct cleared objects
}