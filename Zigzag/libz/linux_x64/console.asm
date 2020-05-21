section .text

; rdi: Text
; rsi: Length
global sys_print
sys_print:
mov rdx, rsi
mov rsi, rdi
mov rdi, 1
mov rax, 1 ; sys_write
syscall
ret

; rdi: Buffer
; rsi: Length
global sys_read
sys_read:
mov rdx, rsi
mov rsi, rdi
xor rdi, rdi
xor rax, rax ; sys_read
syscall
ret