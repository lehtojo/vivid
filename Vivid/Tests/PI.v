Array<T> {
	data: link<T>
	count: large
	
	init(count: large) {
		this.data = allocate(count * sizeof(T))
		this.count = count
	}
	
	set(i: large, value: T) {
		data[i] = value
	}
	
	get(i: large) {
		=> data[i]
	}
	
	deinit() {
		deallocate(data, count)
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

		pi[i] = x[l - 1] / 10

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