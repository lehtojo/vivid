f(x: (num) => bool) {
	=> x(7)
}

exists(x) => x == 7

init() {
	x = 3

	f(h => (h * 2) as bool)

	y = (a: num) => a + x - 1
	y(10)

	worker = Worker(12)
	
	if exists(10) {
		x = 7
	}
	else {
		x = -1
	}
}

Worker {
	task: (num) => num
	i: num
	value: num

	init(value) {
		this.value = value
		w = value * 2

		task = (i: num) => {
			if this.value == i and w == 10 {
				=> 1
			}
			else {
				=> 0
			}
		}

		task(w)
	}
}