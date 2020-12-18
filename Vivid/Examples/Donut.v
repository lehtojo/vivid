import sleep(x: large)
import fill(destination: link, count: large, value: large)

init() {
	A = 0.0
	B = 0.0

	z = allocate<decimal>(1760)
	b = allocate(1760)

	print('\x1b[2J')

	loop {

		fill(b, 1760, 32)
		fill(z, 1760 * sizeof(decimal), 0)

		loop (j = 0.0, j < 6.28, j += 0.07) {
			loop (i = 0.0, i < 6.28, i += 0.02) {
				c = sin(i)
				d = cos(j)
				e = sin(A)
				f = sin(j)
				g = cos(A)
				h = d + 2
				D = 1 / (c * h * e + f * g + 5)
				l = cos(i)
				m = cos(B)
				n = sin(B)
				t = c * h * g - f * e
				x = (40 + 30.0 * D * (l * h * m - t * n)) as large
				y = (12 + 15.0 * D * (l * h * n + t * m)) as large
				o = (x + 80 * y) as large
				N = (8.0 * ((f * e - c * d * g) * m - c * d * e - f * g - l * d * n)) as large

				if 22 > y and y > 0 and x > 0 and 80 > x and D > z[o] {
					z[o] = D

					if N > 0 {
						b[o] = '.,-~:;=!*#$@'[N]
					}
					else {
						b[o] = `.`
					}
				}
			}
		}

		print('\x1b[H')

		loop (k = 0, k < 1761, k++) {

			if k % 80 {
				print_character(b[k])
			}
			else {
				print_character(10)
			}
			
			A += 0.00004
			B += 0.00002
		}

		sleep(2)
	}

	=> 0
}