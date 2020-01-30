\ sandcommands.fs

\    Copyright (C) 2020  Philip King Smith

\    This program is free software: you can redistribute it and/or modify
\    it under the terms of the GNU General Public License as published by
\    the Free Software Foundation, either version 3 of the License, or
\    (at your option) any later version.

\    This program is distributed in the hope that it will be useful,
\    but WITHOUT ANY WARRANTY; without even the implied warranty of
\    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\    GNU General Public License for more details.

\    You should have received a copy of the GNU General Public License
\    along with this program.  If not, see <http://www.gnu.org/licenses/>.

\ commands used by socket server to control sandtable

\ Requires:
\ will be included by socksandserver.fs
\ forth-packages/multi-tasking/0.4.0/multi-tasking.fs \ from theforth.net multi-tasking 0.4.0 package

\ Revisions:
\ 01/29/2020 started coding

require forth-packages/multi-tasking/0.4.0/multi-tasking.fs

only forth also
wordlist constant commands-slow
wordlist constant commands-instant

variable lastresult$


sandtablecommands set-current
