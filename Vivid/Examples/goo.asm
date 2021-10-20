.section text
debug_file_2_start:
.debug_file '/home/lehtojo/vivid/Vivid/Examples/goo.v'

.export _V3goov_rx
.debug_start _V3goov_rx
.loc 1 1 1
sub rsp, 8
.debug_frame_offset 16
.loc 1 2 1
mov rax, 42
.loc 1 3 1
add rsp, 8
ret
_V3goov_rx_end:
.debug_end

debug_file_2_end:

.section debug_abbrev
.byte 1
.byte 17
.byte 1
.byte 37
.byte 8
.byte 19
.byte 5
.byte 3
.byte 8
.byte 16
.byte 23
.byte 27
.byte 8
.byte 17
.byte 1
.byte 18
.byte 6
.byte 0
.byte 0
.byte 2
.byte 2
.byte 1
.byte 54
.byte 11
.byte 3
.byte 8
.byte 11
.byte 6
.byte 58
.byte 6
.byte 59
.byte 6
.byte 0
.byte 0
.byte 3
.byte 2
.byte 0
.byte 54
.byte 11
.byte 3
.byte 8
.byte 11
.byte 6
.byte 58
.byte 6
.byte 59
.byte 6
.byte 0
.byte 0
.byte 4
.byte 36
.byte 0
.byte 3
.byte 8
.byte 62
.byte 11
.byte 11
.byte 6
.byte 0
.byte 0
.byte 5
.byte 15
.byte 0
.byte 73
.byte 19
.byte 0
.byte 0
.byte 6
.byte 13
.byte 0
.byte 3
.byte 8
.byte 73
.byte 19
.byte 58
.byte 6
.byte 59
.byte 6
.byte 56
.byte 6
.byte 50
.byte 11
.byte 0
.byte 0
.byte 7
.byte 5
.byte 0
.byte 2
.byte 24
.byte 3
.byte 8
.byte 58
.byte 6
.byte 59
.byte 6
.byte 73
.byte 19
.byte 0
.byte 0
.byte 8
.byte 52
.byte 0
.byte 2
.byte 24
.byte 3
.byte 8
.byte 58
.byte 6
.byte 59
.byte 6
.byte 73
.byte 19
.byte 0
.byte 0
.byte 9
.byte 1
.byte 1
.byte 73
.byte 19
.byte 0
.byte 0
.byte 10
.byte 33
.byte 0
.byte 73
.byte 19
.byte 55
.byte 5
.byte 0
.byte 0
.byte 11
.byte 28
.byte 0
.byte 73
.byte 19
.byte 56
.byte 6
.byte 50
.byte 11
.byte 0
.byte 0
.byte 12
.byte 46
.byte 1
.byte 17
.byte 1
.byte 18
.byte 6
.byte 64
.byte 24
.byte 3
.byte 8
.byte 58
.byte 6
.byte 59
.byte 6
.byte 73
.byte 6
.byte 0
.byte 0
.byte 0

.section debug_info
debug_info_start:
.dword debug_info_end - debug_info_version
debug_info_version:
.word 4
.dword debug_abbrev
.byte 8
.byte 1
.characters 'Vivid version 1.0'
.byte 0
.word 30583
.characters 'goo.v'
.byte 0
.dword debug_line
.characters '/home/lehtojo/vivid/Vivid'
.byte 0
.qword debug_file_2_start
.dword debug_file_2_end - debug_file_2_start
.byte 12
.qword _V3goov_rx
.dword _V3goov_rx_end - _V3goov_rx
.byte 1
.byte 87
.characters 'init(): large'
.byte 0
.dword 1
.dword 1
.dword _Vx - debug_info_start
.byte 8
.byte 2
.byte 145
.byte 8
.characters 'a'
.byte 0
.dword 1
.dword 2
.dword _Vx - debug_info_start
.byte 8
.byte 2
.byte 145
.byte 0
.characters 'b'
.byte 0
.dword 1
.dword 3
.dword _Vx - debug_info_start
.byte 0
_Vx:
.byte 4
.characters 'large'
.byte 0
.byte 5
.dword 8
.byte 0
.byte 0
debug_info_end: