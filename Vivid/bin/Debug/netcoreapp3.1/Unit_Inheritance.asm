section .text
global main
main:
jmp _V4initv_rc

extern _V8allocatex_rPh

global _V10get_animalv_rP6Animal
export _V10get_animalv_rP6Animal
_V10get_animalv_rP6Animal:
sub rsp, 40
call _VN6Animal4initEv_rPh
add rsp, 40
ret

global _V8get_fishv_rP4Fish
export _V8get_fishv_rP4Fish
_V8get_fishv_rP4Fish:
sub rsp, 40
call _VN4Fish4initEv_rPh
add rsp, 40
ret

global _V10get_salmonv_rP6Salmon
export _V10get_salmonv_rP6Salmon
_V10get_salmonv_rP6Salmon:
sub rsp, 40
call _VN6Salmon4initEv_rPh
add rsp, 40
ret

global _V12animal_movesP6Animal
export _V12animal_movesP6Animal
_V12animal_movesP6Animal:
sub rsp, 40
call _VN6Animal4moveEv
add rsp, 40
ret

global _V10fish_movesP4Fish
export _V10fish_movesP4Fish
_V10fish_movesP4Fish:
sub rsp, 40
lea r8, [rcx-3]
movsx rdx, byte [r8+9]
test rdx, rdx
jne _V10fish_movesP4Fish_L0
lea rdx, [rcx-3]
call _VN4Fish4swimEP6Animal
_V10fish_movesP4Fish_L0:
add rsp, 40
ret

global _V10fish_swimsP6Animal
export _V10fish_swimsP6Animal
_V10fish_swimsP6Animal:
sub rsp, 40
mov rdx, rcx
mov r8, rcx
lea rcx, [r8+3]
call _VN4Fish4swimEP6Animal
add rsp, 40
ret

global _V10fish_stopsP6Animal
export _V10fish_stopsP6Animal
_V10fish_stopsP6Animal:
sub rsp, 40
mov rdx, rcx
lea rcx, [rdx+3]
call _VN4Fish5floatEv
add rsp, 40
ret

global _V10fish_hidesP6Salmon
export _V10fish_hidesP6Salmon
_V10fish_hidesP6Salmon:
push rbx
sub rsp, 48
mov rbx, rcx
lea rcx, [rbx+3]
call _V10fish_movesP4Fish
mov rcx, rbx
call _VN6Salmon4hideEv
add rsp, 48
pop rbx
ret

global _V17fish_stops_hidingP6Salmon
export _V17fish_stops_hidingP6Salmon
_V17fish_stops_hidingP6Salmon:
push rbx
sub rsp, 48
mov rbx, rcx
call _VN6Salmon11stop_hidingEv
lea rcx, [rbx+3]
mov rdx, rbx
call _VN4Fish4swimEP6Animal
add rsp, 48
pop rbx
ret

_V4initv_rc:
mov rax, 1
ret

_VN6Animal4initEv_rPh:
sub rsp, 40
mov rcx, 3
call _V8allocatex_rPh
mov word [rax], 100
mov byte [rax+2], 0
add rsp, 40
ret

_VN6Animal4moveEv:
sub word [rcx], 1
add byte [rcx+2], 1
ret

_VN4Fish4initEv_rPh:
sub rsp, 40
mov rcx, 6
call _V8allocatex_rPh
mov word [rax], 1
mov word [rax+2], 0
mov word [rax+4], 1500
add rsp, 40
ret

_VN4Fish4swimEP6Animal:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, rdx
call _VN6Animal4moveEv
mov cx, [rbx]
mov word [rbx+2], cx
add rsp, 48
pop rbx
ret

_VN4Fish5floatEv:
mov word [rcx+2], 0
ret

_VN6Salmon4initEv_rPh:
sub rsp, 40
mov rcx, 10
call _V8allocatex_rPh
mov byte [rax+9], 0
mov word [rax+3], 5
mov word [rax+7], 5000
add rsp, 40
ret

_VN6Salmon4hideEv:
push rbx
sub rsp, 48
mov rbx, rcx
lea rcx, [rbx+3]
call _VN4Fish5floatEv
mov byte [rbx+9], 1
add rsp, 48
pop rbx
ret

_VN6Salmon11stop_hidingEv:
push rbx
sub rsp, 48
mov rdx, rcx
mov rbx, rcx
lea rcx, [rbx+3]
call _VN4Fish4swimEP6Animal
mov byte [rbx+9], 0
add rsp, 48
pop rbx
ret

_VN11Salmon_Gang4initEx_rPh:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, 8
call _V8allocatex_rPh
mov qword [rax], 1
mov qword [rax], rbx
add rsp, 48
pop rbx
ret