[section .text]

global allocate
allocate:
push rdi
push rsi
push rbx

;   System call parameters
;
;   off_t offset,
;   int fd, 
;   int flags,    
;   int prot, 
;   size_t length, 
;   void *addr
;
xor rdi, rdi ; Address
mov rsi, [rsp+32] ; Length
mov rdx, 0x03 ; PERMISSION_READ | PERMISSION_WRITE
mov r10, 0x22 ; HARDWARE_MEMORY | VISIBILITY_PRIVATE
mov r8, -1 ; FD
xor r9, r9

; System call: mmap
mov rax, 0x09
mov rbx, rsp
syscall
mov rsp, rbx

; Clean and exit
pop rbx
pop rsi
pop rdi
ret 8

global deallocate
deallocate:
push rdi
push rsi
push rbx

mov rax, 11
mov rdi, [rsp+32]
mov rsi, [rsp+40]

; System call: munmap
mov rax, 0x09
mov rbx, rsp
syscall
mov rsp, rbx

pop rbx
pop rsi
pop rdi
ret 16

global copy
copy:

; Parameters
; rsp+24: destination
; rsp+16: count
; rsp+8: source
; rsp+0: return address

pop r8 ; Save return address

pop r9 ; Load source
pop rcx ; Load count
pop r10 ; Load destination

push rsi ; Save non-volatile register RSI
push rdi ; Save non-volatile register RDI

mov rsi, r9 ; Relocate source
mov rdi, r10 ; Relocate destination

rep movsb

pop rdi ; Recover non-volatile register RSI
pop rsi ; Recover non-volatile register RDI

jmp r8

global offset_copy
offset_copy:

; Parameters
; rsp+32: offset
; rsp+24: destination
; rsp+16: count
; rsp+8: source
; rsp+0: return address

pop r8 ; Save return address

pop r9 ; Load source
pop rcx ; Load count
pop r10 ; Load destination
pop r11 ; Load offset

push rsi ; Save non-volatile register RSI
push rdi ; Save non-volatile register RDI

mov rsi, r9 ; Relocate source
mov rdi, r10 ; Relocate destination
add rdi, r11 ; Add the destination offset

rep movsb

pop rdi ; Recover non-volatile register RSI
pop rsi ; Recover non-volatile register RDI

jmp r8

global zero
zero:

; Parameters
; rsp+16: count
; rsp+8: destination
; rsp+0: return address

pop r8 ; Load return address
pop r9 ; Load destination
pop rcx ; Load count

push rdi ; Save non-volatile register RDI
mov rdi, r9 ; Relocate destination

xor rax, rax ; RAX contains the value to copy (zero)

rep stosb

pop rdi ; Recover non-volatile register RDI

jmp r8

global fill
fill:

; Parameters
; esp+12: value
; esp+8: count
; esp+4: destination
; esp+0: return address

pop r8 ; Load return address
pop r9 ; Load destination
pop rcx ; Load count
pop rax ; Load value

push rdi ; Save non-volatile register RDI
mov rdi, r9 ; Relocate destination

rep stosb

pop rdi ; Recover non-volatile register RDI

jmp r8