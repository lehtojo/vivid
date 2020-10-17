import socket(address_family: num, socket_type: num, protocol: num): num
import inet_addr(ip: link): u32
import htons(port: num): u16
import connect(description: normal, address: SocketAddressDescription, size: num): num
import send(description: normal, data: link, size: num, flags: num): num
import recv(description: normal, buffer: link, capacity: num, flags: num): num
import close(description: num)

ADDRESS_FAMILY_INET = 2
ADDRESS_FAMILY_INET6 = 10

SOCKET_TYPE_STREAM = 1
SOCKET_TYPE_DATAGRAM = 2

true = 1
false = 0
none = 0

SocketAddressDescription {
   family: small
   port: u16
   address: u32
   zero: u64
}

Socket {
   private description: normal

   init() {
	  description = socket(ADDRESS_FAMILY_INET, SOCKET_TYPE_STREAM, 0)

	  if description == -1 {
		 println('Could not create a socket')
		 =>
	  }
   }

   connect_to(ip: link, port: num) {
	  if ip == none or port < 0 {
		 println('Invalid connection options passed to a socket')
		 => false
	  }

	  address_description = SocketAddressDescription()
	  address_description.address = inet_addr(ip)
	  address_description.family = ADDRESS_FAMILY_INET
	  address_description.port = port

	  if connect(description, address_description, 16) < 0 {
		 println('Could not connect a socket')
		 => false
	  }

	  => true
   }

   send(data: link, size: num) {
	  if data == none or size < 0 {
		 println('Invalid send options passed to a socket')
		 => false
	  }

	  if send(description, data, size, 0) < 0 {
		 println('Could not send socket data')
		 => false
	  }

	  => true
   }

   receive(buffer: link, capacity: num) {
	  if buffer == none or capacity < 0 {
		 println('Invalid receive options passed to a socket')
		 => false
	  }

	  if recv(description, buffer, capacity, 0) < 0 {
		 println('Could not receive socket data')
		 => false
	  }

	  => true
   }

   close() {
	  close(description)
   }

   deinit() {
	  close()
   }
}

import sleep(seconds: u32)

init() {
   socket = Socket()

   loop (socket.connect_to('192.168.43.112', 7777) == false) {}

   loop {
	  socket.send('Hola', 4)
	  sleep(1)
   }
}