fibonacci(iterations) {
	i = 0
	first = 0
	second = 1
	next = 0

	loop (i < iterations) {
		if i <= 1 {
			next = i
		} 
		else {
			next = first + second
			first = second
			second = next
		}

		println(to_string(next))

		++i
	}
}

init() {
	fibonacci(10)
	=> 0
}