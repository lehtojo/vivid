export StringBuilder {
	private capacity: large
	private position: large

	buffer: link
	length => position

	init() {
		capacity = 1
		buffer = allocate(1)
	}

	init(value: String) {
		capacity = value.length
		buffer = allocate(value.length)
		append(value)
	}

	private grow(requirement: large) {
		capacity: large = (position + requirement) * 2
		buffer: link = allocate(capacity)

		copy(this.buffer, position, buffer)
		deallocate(this.buffer)

		this.capacity = capacity
		this.buffer = buffer
	}

	append(text: link, length: large) {
		if length == 0 return

		if position + length > capacity grow(length)

		offset_copy(text, length, buffer, position)
		position += length
	}

	append(text: link) {
		length = length_of(text)
		append(text, length)
	}

	append(text: String) {
		append(text.text, text.length)
	}

	append(value: large) {
		append(to_string(value))
	}

	append(value: decimal) {
		append(to_string(value))
	}

	append_line(text: link) {
		append(text)
		append(`\n`)
	}

	append_line(text: String) {
		append(text.text, text.length)
		append(`\n`)
	}

	append_line(text: large) {
		append_line(to_string(text))
	}

	append_line(text: decimal) {
		append_line(to_string(text))
	}

	append_line(character: char) {
		append(character)
		append(`\n`)
	}

	append(character: char) {
		if position + 1 > capacity grow(1)
		buffer[position] = character
		position++
	}

	remove(start: large, end: large) {
		count = end - start
		if count == 0 return
		
		move(buffer + end, buffer + start, position - end)

		position -= count
	}

	insert(index: large, text: link, length: large) {
		if length == 0 return
		if position + length > capacity grow(length)

		move(buffer + index, buffer + index + length, position - index)
		offset_copy(text, length, buffer, index)
		position += length
	}

	insert(index: large, text: link) {
		=> insert(index, text, length_of(text))
	}

	replace(from: link, to: link) {
		a = length_of(from)
		b = length_of(to)

		if a == 0 return

		loop (i = position - a, i >= 0, i--) {
			match = true

			loop (j = 0, j < a, j++) {
				if buffer[i + j] == from[j] continue
				match = false
				stop
			}

			if not match continue

			remove(i, i + a)
			insert(i, to)
		}
	}

	reverse() {
		count = position / 2

		loop (i = 0, i < count, i++) {
			temporary = buffer[i]
			buffer[i] = buffer[position - i - 1]
			buffer[position - i - 1] = temporary
		}
	}

	get(i: large) {
		=> buffer[i]
	}

	string() {
		=> String(buffer, position)
	}
}