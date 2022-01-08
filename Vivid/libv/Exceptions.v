export Outcome<T, E> {
	value: u64
	is_error: bool

	has_value() => !is_error
	get_value() => value as T

	# Summary: Returns the specified fallback value if the outcome represents an error, otherwise the contained value is returned
	value_or(fallback: T) {
		result = value
		if is_error { result = fallback }
		=> result
	}
}

export Outcome<T, E> Ok<T, E> {
	init(value: T) {
		this.value = value as u64
		this.is_error = false
	}
}

export Outcome<T, E> Error<T, E> {
	init(value: E) {
		this.value = value as u64
		this.is_error = true
	}

	get_error() => value as E
}

export Optional<T> {
	value: T
	empty: bool

	init() {
		empty = true
	}

	init(value: T) {
		this.value = value
		this.empty = false
	}

	has_value() => !empty
	get_value() => value

	value_or(fallback: T) {
		result = value
		if empty { result = fallback }
		=> result
	}
}

export panic(message: link) {
	internal.console.write(message, length_of(message))
	application.exit(1)
}