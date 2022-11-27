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

# Summary:
# - Replaces all `\\` with `/`
# - Replaces all sequential `/` with a single `/`
export normalise(path: String) {
	result = StringBuilder(path.replace(`\\`, `/`))

	loop (i = 0, i < result.length, i++) {
		if result[i] !== `/` continue

		# Find the index of the next character that is not a slash
		j = i + 1
		loop (j < result.length and result[j] === `/`, j++) {}

		# Remove all slashes after the current slash
		result.remove(i + 1, j)
	}

	return result.string()
}

# Summary:
# Corresponds to: path.normalise(first + second)
export join(first: String, second: String) {
	return path.normalise(first + second)
}