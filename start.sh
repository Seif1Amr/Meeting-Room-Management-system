#!/usr/bin/env bash
set -e

echo "============================================================"
echo "  Launching MeetingHub Full-Stack Application"
echo "============================================================"

# Trap SIGINT to cleanly kill child processes
cleanup() {
    echo ""
    echo "Stopping MeetingHub services..."
    kill "$BACKEND_PID" 2>/dev/null || true
    exit 0
}
trap cleanup SIGINT SIGTERM EXIT

echo ""
echo "1. Starting .NET 8 Backend API on http://localhost:5000..."
dotnet run --project backend/MeetingRoomManagement.Api.csproj --urls http://localhost:5000 &
BACKEND_PID=$!

echo "   -> Swagger API documentation: http://localhost:5000/swagger"

echo ""
echo "2. Starting Angular Frontend on http://localhost:4200..."
cd frontend
if [ ! -d "node_modules" ]; then
    echo "   Installing frontend dependencies (first-time run)..."
    npm install --legacy-peer-deps
fi
echo "   -> Frontend web application: http://localhost:4200"
echo ""
echo "Press Ctrl+C to stop both frontend and backend."
echo ""
npm start
