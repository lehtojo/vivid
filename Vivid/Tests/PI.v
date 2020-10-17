Array<T> {
	private data: link
	count: num
	
	init(count: num) {
		this.data = allocate(count * T.size)
		this.count = count
	}
	
	set(i: num, value: T) {
		data[i * T.size] as T = value
	}
	
	get(i: num) {
		=> data[i * T.size] as T
	}
	
	deinit() {
		deallocate(data, count * T.size)
	}
}

pidigits(digits) {   
	++digits

	l = digits * 10 / 3 + 2
	
	x = Array<large>(l)
	r = Array<large>(l)

	pi = Array<large>(digits)

	j = 0

	loop (j, j < l, ++j) {
		x[j] = 20
	}

	i = 0

	loop (i, i < digits, ++i) {
		carry = 0
		j = 0

		loop (j, j < l, ++j) {
			num = l - j - 1
			dem = num * 2 + 1

			x[j] = x[j] + carry

			q = x[j] / dem
			r[j] = x[j] % dem

			carry = q * num
		}

		pi[i] = x[l-1] / 10

		r[l - 1] = x[l - 1] % 10

		j = 0
		loop (j, j < l, ++j) {
			x[j] = r[j] * 10
		}
	}

	result = Array<u8>(digits * 8)

	c = 0
	i = digits - 1

	loop (i >= 0) {
		pi[i] = pi[i] + c
		c = pi[i] / 10

		result[i] = 48 + pi[i] % 10

		i -= 1
	}
	
	=> result.data
}

init() {
	pi = pidigits(3141)
	print(pi)
	=> 0
}