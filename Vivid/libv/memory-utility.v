export move(source: link, offset: large, destination: link, bytes: large) {
	# Copy the area to be moved to a temporary buffer, since moving can override the bytes to be moved
	buffer = allocate(bytes)
	source += offset
	copy(source, bytes, buffer)

	# Zero the area to be moved
	zero(source, bytes)
	
	# Copy the contents of the temporary buffer to the destination
	copy(buffer, bytes, destination)

	# Delete the temporary buffer
	deallocate(buffer)
}

export move(source: link, destination: link, bytes: large) {
	# Copy the area to be moved to a temporary buffer, since moving can override the bytes to be moved
	buffer = allocate(bytes)
	copy(source, bytes, buffer)

	# Zero the area to be moved
	zero(source, bytes)

	# Copy the contents of the temporary buffer to the destination
	copy(buffer, bytes, destination)

	# Delete the temporary buffer
	deallocate(buffer)
}

# Summary: Allocates a new buffer, with the size of 'to' bytes, and copies the contents of the source buffer to the new buffer. Also deallocates the source buffer.
export resize(source: link, from: large, to: large) {
	resized = allocate(to)
	copy(source, min(from, to), resized)
	deallocate(source)
	return resized
}

# Summary: Reverses the bytes in the specified memory range
export reverse(memory: link, size: large) {
	i = 0
	j = size - 1
	n = size / 2

	loop (i < n) {
		temporary = memory[i]
		memory[i] = memory[j]
		memory[j] = temporary

		i++
		j--
	}
}

export outline copy<T>(destination: T*, source: T*, size: large) {
	loop (i = 0, i < size, i++) {
		destination[i] = source[i]
	}
}

export outline zero<T>(destination: T*, size: large) {
	loop (i = 0, i < size, i++) {
		destination[i] = 0 as T
	}
}