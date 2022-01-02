init() {
	=> 1
}

namespace application

import 'C' ExitProcess(status: large)

export exit(status: large) {
	ExitProcess(status)
}