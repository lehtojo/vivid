export move(source: link, offset: large, destination: link, bytes: large) {
	# Copy the area to be moved to a temporary buffer, since moving can override the bytes to be moved
	buffer = allocate(bytes)
	source += offset
	copy(source, bytes, buffer)
	
	# Copy the contents of the temporary buffer to the destination
	copy(buffer, bytes, destination)

	# Delete the temporary buffer
	deallocate(buffer)
}

export move(source: link, destination: link, bytes: large) {
	# Copy the area to be moved to a temporary buffer, since moving can override the bytes to be moved
	buffer = allocate(bytes)
	copy(source, bytes, buffer)

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
	=> resized
}