#!/usr/bin/env python3
"""Serve Unity WebGL build with correct gzip headers for .gz files."""
import http.server
import socketserver
import os
import sys

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 8080


class WebGLHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        if self.path.endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")
        self.send_header("Cross-Origin-Embedder-Policy", "require-corp")
        self.send_header("Cross-Origin-Opener-Policy", "same-origin")
        super().end_headers()


if __name__ == "__main__":
    os.chdir(os.path.dirname(os.path.abspath(__file__)))
    print(f"Serving WebGL at http://localhost:{PORT}")
    print("Press Ctrl+C to stop.")
    with socketserver.TCPServer(("", PORT), WebGLHandler) as httpd:
        httpd.serve_forever()
