import time(): large

init() {
	p = time()
	i = 0

	loop {
		n = time()

		if n - p >= 10000000 {
			print(++i)
			println(' second(s) has passed')
			p = n
		}
	}
}