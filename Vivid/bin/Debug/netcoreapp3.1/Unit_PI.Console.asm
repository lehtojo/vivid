.section .text
.intel_syntax noprefix
.global _V5printPh
_V5printPh:
push rbx
sub rsp, 32
mov rbx, rcx
call _V9length_ofPh_rx
mov rcx, rbx
mov rdx, rax
call _V14internal_printPhx
add rsp, 32
pop rbx
ret

.section .data

