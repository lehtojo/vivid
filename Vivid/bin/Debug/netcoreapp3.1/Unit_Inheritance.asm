.section .text
.intel_syntax noprefix
.file 1 "Sandbox.v"
.global main
main:
jmp _V4initv_rc

.extern _V17internal_allocatex_rPh

.global _V10get_animalv_rP6Animal
_V10get_animalv_rP6Animal:
sub rsp, 40
call _VN6Animal4initEv_rPS_
add rsp, 40
ret

.global _V8get_fishv_rP4Fish
_V8get_fishv_rP4Fish:
sub rsp, 40
call _VN4Fish4initEv_rPS_
add rsp, 40
ret

.global _V10get_salmonv_rP6Salmon
_V10get_salmonv_rP6Salmon:
sub rsp, 40
call _VN6Salmon4initEv_rPS_
add rsp, 40
ret

.global _V12animal_movesP6Animal
_V12animal_movesP6Animal:
sub rsp, 40
call _VN6Animal4moveEv
add rsp, 40
ret

.global _V10fish_movesP4Fish
_V10fish_movesP4Fish:
push rbx
sub rsp, 32
mov rbx, rcx
lea rdx, [rbx-11]
movsx rcx, byte ptr [rdx+33]
test rcx, rcx
jne _V10fish_movesP4Fish_L0
mov rcx, rbx
lea rdx, [rbx-11]
call _VN4Fish4swimEP6Animal
_V10fish_movesP4Fish_L0:
add rsp, 32
pop rbx
ret

.global _V10fish_swimsP6Animal
_V10fish_swimsP6Animal:
sub rsp, 40
mov rdx, rcx
mov r8, rcx
lea rcx, [r8+11]
call _VN4Fish4swimEP6Animal
add rsp, 40
ret

.global _V10fish_stopsP6Animal
_V10fish_stopsP6Animal:
sub rsp, 40
mov rdx, rcx
lea rcx, [rdx+11]
call _VN4Fish5floatEv
add rsp, 40
ret

.global _V10fish_hidesP6Salmon
_V10fish_hidesP6Salmon:
push rbx
sub rsp, 32
mov rbx, rcx
lea rcx, [rbx+11]
call _V10fish_movesP4Fish
mov rcx, rbx
call _VN6Salmon4hideEv
add rsp, 32
pop rbx
ret

.global _V17fish_stops_hidingP6Salmon
_V17fish_stops_hidingP6Salmon:
push rbx
sub rsp, 32
mov rbx, rcx
call _VN6Salmon11stop_hidingEv
lea rcx, [rbx+11]
mov rdx, rbx
call _VN4Fish4swimEP6Animal
add rsp, 32
pop rbx
ret

_V4initv_rc:
mov rax, 1
ret

_V8allocatex_rPh:
push rbx
push rsi
sub rsp, 40
mov r8, [rip+_VN10Allocation_current]
test r8, r8
je _V8allocatex_rPh_L0
mov rdx, [r8+16]
lea r9, [rdx+rcx]
cmp r9, 1000000
jg _V8allocatex_rPh_L0
lea r9, [rdx+rcx]
mov qword ptr [r8+16], r9
lea r9, [rdx+rcx]
mov rax, [r8+8]
add rax, rdx
add rsp, 40
pop rsi
pop rbx
ret
_V8allocatex_rPh_L0:
mov rbx, rcx
mov rcx, 1000000
call _V17internal_allocatex_rPh
mov rcx, 24
mov rsi, rax
call _V17internal_allocatex_rPh
mov qword ptr [rax+8], rsi
mov qword ptr [rax+16], rbx
mov qword ptr [rip+_VN10Allocation_current], rax
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_V8inheritsPhS__rc:
push rbx
push rsi
mov r8, [rcx]
mov r9, [rdx]
movzx r10, byte ptr [r9]
xor rax, rax
_V8inheritsPhS__rc_L1:
_V8inheritsPhS__rc_L0:
movzx rcx, byte ptr [r8+rax]
add rax, 1
cmp rcx, r10
jnz _V8inheritsPhS__rc_L4
mov r11, rcx
mov rbx, 1
_V8inheritsPhS__rc_L7:
_V8inheritsPhS__rc_L6:
movzx r11, byte ptr [r8+rax]
movzx rsi, byte ptr [r9+rbx]
add rax, 1
add rbx, 1
cmp r11, rsi
jz _V8inheritsPhS__rc_L9
cmp r11, 1
jne _V8inheritsPhS__rc_L9
test rsi, rsi
jne _V8inheritsPhS__rc_L9
mov rax, 1
pop rsi
pop rbx
ret
_V8inheritsPhS__rc_L9:
jmp _V8inheritsPhS__rc_L6
_V8inheritsPhS__rc_L8:
jmp _V8inheritsPhS__rc_L3
_V8inheritsPhS__rc_L4:
cmp rcx, 2
jne _V8inheritsPhS__rc_L3
xor rax, rax
pop rsi
pop rbx
ret
_V8inheritsPhS__rc_L3:
jmp _V8inheritsPhS__rc_L0
_V8inheritsPhS__rc_L2:
pop rsi
pop rbx
ret

_VN6Animal4initEv_rPS_:
sub rsp, 40
mov rcx, 11
call _V8allocatex_rPh
mov byte ptr [rax+10], 0
mov word ptr [rax+8], 100
add rsp, 40
ret

_VN6Animal4moveEv:
sub word ptr [rcx+8], 1
add byte ptr [rcx+10], 1
ret

_VN4Fish4initEv_rPS_:
sub rsp, 40
mov rcx, 14
call _V8allocatex_rPh
mov word ptr [rax+12], 1500
mov word ptr [rax+10], 0
mov word ptr [rax+8], 1
add rsp, 40
ret

_VN4Fish4swimEP6Animal:
push rbx
sub rsp, 32
mov rbx, rcx
mov rcx, rdx
call _VN6Animal4moveEv
movsx rcx, word ptr [rbx+8]
mov word ptr [rbx+10], cx
add rsp, 32
pop rbx
ret

_VN4Fish5floatEv:
mov word ptr [rcx+10], 0
ret

_VN6Salmon4initEv_rPS_:
sub rsp, 40
mov rcx, 34
call _V8allocatex_rPh
lea rcx, [rip+_VN6Salmon_configuration+16]
mov qword ptr [rax+11], rcx
lea rcx, [rip+_VN6Salmon_configuration+8]
mov qword ptr [rax], rcx
mov byte ptr [rax+33], 0
mov word ptr [rax+19], 5
mov word ptr [rax+23], 5000
add rsp, 40
ret

_VN6Salmon4hideEv:
push rbx
sub rsp, 32
mov rbx, rcx
lea rcx, [rbx+11]
call _VN4Fish5floatEv
mov byte ptr [rbx+33], 1
add rsp, 32
pop rbx
ret

_VN6Salmon11stop_hidingEv:
push rbx
sub rsp, 32
mov rdx, rcx
mov rbx, rcx
lea rcx, [rbx+11]
call _VN4Fish4swimEP6Animal
mov byte ptr [rbx+33], 0
add rsp, 32
pop rbx
ret

_VN11Salmon_Gang4initEx_rPS_:
push rbx
sub rsp, 32
mov rbx, rcx
mov rcx, 16
call _V8allocatex_rPh
mov qword ptr [rax+8], 1
mov qword ptr [rax+8], rbx
add rsp, 32
pop rbx
ret

.section .data

_VN10Allocation_current:
.quad 0

_VN6Animal_configuration:
.quad _VN6Animal_descriptor

_VN6Animal_descriptor:
.quad _VN6Animal_descriptor_0
.long 11
.long 0

_VN6Animal_descriptor_0:
.ascii "Animal"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4Fish_configuration:
.quad _VN4Fish_descriptor

_VN4Fish_descriptor:
.quad _VN4Fish_descriptor_0
.long 14
.long 0

_VN4Fish_descriptor_0:
.ascii "Fish"
.byte 0
.byte 1
.byte 2
.byte 0

_VN6Salmon_configuration:
.quad _VN6Salmon_descriptor
.quad _VN6Salmon_descriptor
.quad _VN6Salmon_descriptor

_VN6Salmon_descriptor:
.quad _VN6Salmon_descriptor_0
.long 34
.long 2
.quad _VN6Animal_descriptor
.quad _VN4Fish_descriptor

_VN6Salmon_descriptor_0:
.ascii "Salmon"
.byte 0
.byte 1
.ascii "Animal"
.byte 1
.ascii "Fish"
.byte 1
.byte 2
.byte 0

_VN11Salmon_Gang_configuration:
.quad _VN11Salmon_Gang_descriptor

_VN11Salmon_Gang_descriptor:
.quad _VN11Salmon_Gang_descriptor_0
.long 16
.long 0

_VN11Salmon_Gang_descriptor_0:
.ascii "Salmon_Gang"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4Page_configuration:
.quad _VN4Page_descriptor

_VN4Page_descriptor:
.quad _VN4Page_descriptor_0
.long 24
.long 0

_VN4Page_descriptor_0:
.ascii "Page"
.byte 0
.byte 1
.byte 2
.byte 0

_VN10Allocation_configuration:
.quad _VN10Allocation_descriptor

_VN10Allocation_descriptor:
.quad _VN10Allocation_descriptor_0
.long 8
.long 0

_VN10Allocation_descriptor_0:
.ascii "Allocation"
.byte 0
.byte 1
.byte 2
.byte 0