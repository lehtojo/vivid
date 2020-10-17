OS = 'Windows'

if OS == 'Windows' {
   VALUE = 7

   f(x) {
	  => VALUE * x
   }
}
else {
   VALUE = 0

   f(x) {
	  => VALUE * x
   }
}

init() {
   => f(VALUE)
}