.section .text
.intel_syntax noprefix
.global _VN4ListIP7VehicleE4growEv
_VN4ListIP7VehicleE4growEv:
push rbx
push rsi
sub rsp, 40
mov rdx, [rcx+16]
sal rdx, 1
sal rdx, 3
mov rbx, rcx
mov rcx, rdx
call _V8allocatex_rPh
mov rdx, [rbx+16]
sal rdx, 3
mov rcx, [rbx+8]
mov r8, rax
mov rsi, rax
call _V4copyPhxS_
mov qword ptr [rbx+8], rsi
mov rcx, [rbx+16]
sal rcx, 1
mov qword ptr [rbx+16], rcx
add rsp, 40
pop rsi
pop rbx
ret

.global _VN4ListIP7VehicleE3addES0_
_VN4ListIP7VehicleE3addES0_:
push rbx
push rsi
push rdi
sub rsp, 32
mov rsi, rcx
mov rbx, [rsi+24]
cmp rbx, [rsi+16]
jne _VN4ListIP7VehicleE3addES0__L0
mov rcx, rsi
mov rdi, rdx
call _VN4ListIP7VehicleE4growEv
mov rdx, rdi
_VN4ListIP7VehicleE3addES0__L0:
mov rcx, [rsi+8]
mov rbx, [rsi+24]
mov qword ptr [rcx+rbx*8], rdx
lea rcx, [rbx+1]
mov qword ptr [rsi+24], rcx
add rbx, 1
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

.global _VN4ListIP7VehicleE6removeEx
_VN4ListIP7VehicleE6removeEx:
push rbp
push rbx
push rsi
push rdi
sub rsp, 40
lea rbp, [rdx+1]
sal rbp, 3
mov rbx, [rcx+24]
sub rbx, rdx
sub rbx, 1
sal rbx, 3
mov rsi, rcx
mov rdi, rdx
test rbx, rbx
jle _VN4ListIP7VehicleE6removeEx_L0
sal rdi, 3
mov r8, [rsi+8]
add r8, rdi
mov rcx, [rsi+8]
mov rdx, rbp
mov r9, rbx
call _V4movePhxS_x
_VN4ListIP7VehicleE6removeEx_L0:
sub qword ptr [rsi+24], 1
add rsp, 40
pop rdi
pop rsi
pop rbx
pop rbp
ret

.global _VN4ListIP7VehicleE3getEx_rS0_
_VN4ListIP7VehicleE3getEx_rS0_:
mov r8, [rcx+8]
mov rax, [r8+rdx*8]
ret

.global _VN4ListIP7VehicleE4sizeEv_rx
_VN4ListIP7VehicleE4sizeEv_rx:
mov rax, [rcx+24]
ret

.global _VN4ListIP7VehicleE4initEv_rS1_
_VN4ListIP7VehicleE4initEv_rS1_:
push rbx
sub rsp, 32
mov rcx, 32
call _V8allocatex_rPh
mov rcx, 8
mov rbx, rax
call _V8allocatex_rPh
mov qword ptr [rbx+8], rax
mov qword ptr [rbx+16], 1
mov qword ptr [rbx+24], 0
mov rax, rbx
add rsp, 32
pop rbx
ret

.section .data

_VN4List_configuration:
.quad _VN4List_descriptor

_VN4List_descriptor:
.quad _VN4List_descriptor_0
.long 8
.long 0

_VN4List_descriptor_0:
.ascii "List"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4ListIP7VehicleE_configuration:
.quad _VN4ListIP7VehicleE_descriptor

_VN4ListIP7VehicleE_descriptor:
.quad _VN4ListIP7VehicleE_descriptor_0
.long 32
.long 0

_VN4ListIP7VehicleE_descriptor_0:
.ascii "List<Vehicle>"
.byte 0
.byte 1
.byte 2
.byte 0

