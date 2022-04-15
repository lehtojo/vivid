export Set<T> {
	private inline container: List<T>

	# Summary: Creates a list with the specified initial size
	init(size: large, fill: bool) {
		container.init(size, fill)
	}

	# Summary: Creates a list with the specified initial size
	init(elements: link<T>, size: large) {
		container.init(elements, size)
	}

	# Summary: Creates a list with the same contents as the specified list
	init(other: List<T>) {
		container.init(other)
	}

	# Summary: Creates an empty list
	init() {
		container.init()
	}

	contains(element: T) {
		=> container.contains(element)
	}

	add(element: T) {
		if container.contains(element) => false
		container.add(element)
		=> true
	}

	iterator() {
		=> container.iterator()
	}

	to_list() {
		=> List<T>(container)
	}

	to_array() {
		=> container.to_array()
	}
}