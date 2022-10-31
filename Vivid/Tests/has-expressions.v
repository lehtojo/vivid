get<T>(value: T) {
	if value === none return Optional<T>()
	return Optional<T>(value)
}

init() {
	if get<String>(none as String) has not m1 {
		console.write_line("No message :^(")
	} else {
		console.write_line("There is a message :O")
		console.write_line(m1)
	}

	if get<String>("Hello friends :^)") has m2 {
		console.write_line("There is a message:")
		console.write_line(m2)
	} else {
		console.write_line("No message :^(")
	}

	return 0
}