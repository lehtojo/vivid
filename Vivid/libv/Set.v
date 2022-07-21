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
		return container.contains(element)
	}

	add(element: T) {
		if container.contains(element) return false
		container.add(element)
		return true
	}

	iterator() {
		return container.iterator()
	}

	to_list() {
		return List<T>(container)
	}
}