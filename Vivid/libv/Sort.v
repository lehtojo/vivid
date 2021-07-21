sort<T>(elements: link<T>, count: large) {
	quicksort.sort<T>(elements, 0, count - 1)
}

sort<T>(list: List<T>) {
	quicksort.sort<T>(list.elements, 0, list.size - 1)
}

sort<T>(array: Array<T>) {
	quicksort.sort<T>(array.data, 0, list.count - 1)
}

sort<T>(elements: link<T>, count: large, comparator: (T, T) -> large) {
	quicksort.sort<T>(elements, 0, count - 1, comparator)
}

sort<T>(list: List<T>, comparator: (T, T) -> large) {
	quicksort.sort<T>(list.elements, 0, list.size - 1, comparator)
}

sort<T>(array: Array<T>, comparator: (T, T) -> large) {
	quicksort.sort<T>(array.data, 0, list.count - 1, comparator)
}

namespace quicksort

# Summary: Swap the positions of the specified elements
swap(a, b) {
	c = a[0]
	a[0] = b[0]
	b[0] = c
}

partition<T>(elements, low, high) {
	pivot = elements[high]
	i = low - 1 # Indicates the right position of pivot so far

	loop (j = low, j <= high - 1, j++) {
		# If the current element is smaller than the pivot, then update the pivot
		if elements[j] < pivot {
			i++ # Update the pivot
			swap(elements + i * sizeof(T), elements + j * sizeof(T))
		}
	}

	swap(elements + (i + 1) * sizeof(T), elements + high * sizeof(T))
	=> i + 1
}

partition<T>(elements, low, high, comparator) {
	pivot = elements[high]
	i = low - 1 # Indicates the right position of pivot so far

	loop (j = low, j <= high - 1, j++) {
		# If the current element is smaller than the pivot, then update the pivot
		if comparator(elements[j], pivot) < 0 {
			i++ # Update the pivot
			swap(elements + i * sizeof(T), elements + j * sizeof(T))
		}
	}

	swap(elements + (i + 1) * sizeof(T), elements + high * sizeof(T))
	=> i + 1
}

sort<T>(elements, low, high) {
	if low >= high return

	p = partition<T>(elements, low, high)
	sort<T>(elements, low, p - 1)
	sort<T>(elements, p + 1, high)
}

sort<T>(elements, low, high, comparator) {
	if low >= high return

	p = partition<T>(elements, low, high, comparator)
	sort<T>(elements, low, p - 1, comparator)
	sort<T>(elements, p + 1, high, comparator)
}