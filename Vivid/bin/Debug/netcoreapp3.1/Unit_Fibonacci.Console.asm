.section .text
.intel_syntax noprefix
.global _V7printlnP6String
_V7printlnP6String:
push rbx
sub rsp, 32
mov rdx, 10
mov rbx, rcx
call _VN6String6appendEh_rPS_
mov rcx, rax
call _VN6String4dataEv_rPh
mov rcx, rbx
mov rbx, rax
call _VN6String6lengthEv_rx
add rax, 1
mov rcx, rbx
mov rdx, rax
call _V14internal_printPhx
add rsp, 32
pop rbx
ret

.section .data

