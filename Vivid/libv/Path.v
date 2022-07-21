namespace io.path

# Summary: Returns the file name with extension from the specified path
# Examples:
# '/path/to/file.extension' => 'file.extension'
# '/path/to\\file' => 'file'
export basename(path: String) {
	i = path.last_index_of(`/`)
	j = path.last_index_of(`\\`)
	start = max(i, j)

	if start >= 0 return path.slice(start + 1)

	return path
}

# Summary: Returns the full path to the parent folder of the specified path
# Examples:
# '/path/to/file.extension' => '/path/to'
# '/path/to\\file' => '/path/to'
export folder(path: String) {
	i = path.last_index_of(`/`)
	j = path.last_index_of(`\\`)
	start = max(i, j)

	if start >= 0 return path.slice(0, start)

	return path
}