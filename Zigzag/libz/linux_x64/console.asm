section .text

; rdi: Text
; rsi: Length
global _V14internal_printPhx:function hidden
_V14internal_printPhx:
mov rdx, rsi
mov rsi, rdi
mov rdi, 1
mov rax, 1 ; sys_write
syscall
ret

; rdi: Buffer
; rsi: Length
global _V13internal_readPhx_rx:function hidden
_V13internal_readPhx_rx:
mov rdx, rsi
mov rsi, rdi
xor rdi, rdi
xor rax, rax ; sys_read
syscall
ret