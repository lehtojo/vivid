import pow(a: decimal, b: decimal): decimal

import sqrt(a: large): decimal
import sqrt(a: decimal): decimal

import sin(a: decimal): decimal
import cos(a: decimal): decimal
import tan(a: decimal): decimal

export min(a, b) {
	if a < b return a
	else return b
}

export max(a, b) {
	if a > b return a
	else return b
}

export abs(a) {
	if a > 0 return a
	else return -a
}

export ceil(a: decimal) {
	return (a + 0.5) as large
}

export floor(a: decimal) {
	return a as large
}

export sign(a: decimal) {
	if a > 0 return 1
	else a < 0 return -1
	else return 0 
}

export cbrt(a: large) {
	return pow(a, 0.333333333333333)
}

export cbrt(a: decimal) {
	return pow(a, 0.333333333333333)
}