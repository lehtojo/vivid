export SequentialIterator<T> {
	elements: link<T>
	position: normal
	size: normal

	init(elements: link<T>, size: large) {
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