export SequentialIterator<T> {
	elements: T*
	position: normal
	size: normal

	init(elements: T*, size: large) {
		this.elements = elements
		this.position = -1
		this.size = size
	}

	value() {
		return elements[position]
	}

	next() {
		return ++position < size
	}

	reset() {
		position = -1
	}
}