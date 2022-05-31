export LinkedListElement<T> {
	next: LinkedListElement<T>
	value: T

	init(value: T) {
		this.value = value
	}
}

export LinkedList<T> {
	private head: LinkedListElement<T> = none as LinkedListElement<T>
	private tail: LinkedListElement<T> = none as LinkedListElement<T>

	# Summary: Adds the specified element to the end of this list
	add(value: T) {
		element = LinkedListElement<T>(value)

		if tail == none {
			head = element
			tail = element
			return
		}

		tail.next = element
		tail = element
	}

	# Summary: Searches for the specified element from the list and removes it
	remove(value: T) {
		iterator = head
		previous: LinkedListElement<T> = 0

		loop (iterator) {
			if iterator.value == value {
				remove(previous, iterator)
				=> true
			}

			iterator = iterator.next
			previous = iterator
		}

		=> false
	}

	remove(previous: LinkedListElement<T>, iterator: LinkedListElement<T>) {
		if previous {
			previous.next = iterator.next
		}
		else {
			head = iterator.next
		}

		if iterator.next == none {
			tail = previous
		}

		deallocate(iterator as link)
	}

	size() {
		size = 0

		loop (iterator = head, iterator !== none, iterator = iterator.next) {
			size++
		}

		=> size
	}

	first() {
		=> head
	}

	last() {
		=> tail
	}

	iterator() {
		=> head
	}
}