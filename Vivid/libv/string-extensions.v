# Summary: Finds the slices from this string which are separated by the specified character and returns them as an array
String.split(character: char) {
	count = 1

	# Count the number of splits
	loop (index = 0, index < length, index++) {
		if data[index] == character { count++ }
	}

	# Reserve a result list for the slices
	slices = List<String>(count, true)

	slot = 0
	index = 0
	start = 0

	loop (index < length) {
		if data[index] != character {
			index++
			continue
		}
		
		slices[slot++] = String(data + start, index - start)
		start = ++index
	}

	slices[slot] = String(data + start, index - start)
	return slices
}