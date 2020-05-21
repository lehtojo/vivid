[section .text]

; rdi: Length
global allocate
allocate:
;   System call parameters
;
;   off_t offset,
;   int fd, 
;   int flags,    
;   int prot, 
;   size_t length, 
;   void *addr
;
mov rsi, rdi ; Length
xor rdi, rdi ; Address
mov rdx, 0x03 ; PERMISSION_READ | PERMISSION_WRITE
mov r10, 0x22 ; HARDWARE_MEMORY | VISIBILITY_PRIVATE
mov r8, -1 ; FD
xor r9, r9

; System call: mmap
mov rax, 0x09
syscall

ret


; rdi: Address
; rsi: Length
global deallocate
deallocate:

mov rax, 11 ; munmap
syscall

ret

; rdi: Source
; rsi: Count
; rdx: Destination
global copy
copy:

xchg rdi, rdx ; rdi = Destination, rdx = Source
mov rcx, rsi ; rcx = Count
mov rsi, rdx ; rsi = Source

rep movsb

ret

; rdi: Source
; rsi: Count
; rdx: Destination
; r10: Offset
global offset_copy
offset_copy:

add rdx, r10 ; Apply offset

xchg rdi, rdx ; rdi = Destination, rdx = Source
mov rcx, rsi ; rcx = Count
mov rsi, rdx ; rsi = Source

rep movsb

ret

; rdi: Destination
; rsi: Count
global zero
zero:

mov rcx, rsi ; rcx = Count
xor rax, rax ; Value used to fill the range

rep stosb

ret

; rdi: Destination
; rsi: Count
; rdx: Value
global fill
fill:

mov rcx, rsi ; rcx = Count
mov rax, rdx ; rax = Value

rep stosb

ret