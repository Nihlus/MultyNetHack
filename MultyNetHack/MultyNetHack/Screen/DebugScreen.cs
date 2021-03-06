﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultyNetHack;
using MultyNetHack.Commands;

namespace MultyNetHack.Screen
{
    /// <summary>
    /// This should show all debug info (not implemented yet)
    /// </summary>
    class DebugScreen : BaseScreen 
    {
        public DebugScreen(int Top, int Left, BaseScreen ScreenToDebug) : base (Top, Left, string.Format("Debugging {0} screen, it's type of {1}", ScreenToDebug.Name, ScreenToDebug.GetType().ToString().Split(new char[] { '.' }).Last()))
        {
            mInitText(ScreenToDebug);
            Screen_Change(null, EventArgs.Empty);
        }

        private void mInitText(BaseScreen screenToDebug)
        {
            VirtualConsoleAddLine(string.Format("Contains {0} rooms, {1} paths, {2} walls", screenToDebug.NumOfRooms, screenToDebug.NumOfPaths, screenToDebug.NumOfWalls));
            PrintControls(screenToDebug as Component, 1);
            VirtualConsoleAddLine(string.Format("Able to input {0} different commands", screenToDebug.Comand.Count));
            mPrintCommands(screenToDebug.Comand, 1);
        }

        private void mPrintCommands(Dictionary<Comands, Action<BaseCommand>> mCommands, int indent)
        {
            var Km = MultyNetHack.Controls.KeyMap;
            foreach(var mCommand in mCommands)
            {
                VirtualConsoleAddLine(string.Format("{0}{1} -> {2} -> {3}",new string(' ',indent*2), Km.Where(i => i.Value == mCommand.Key).ToList()?[0].Key, mCommand.Key, mCommand.Value.Method.Name));
            }
        }

        private void PrintControls(Component screenToDebug, int indent)
        {
            if (screenToDebug.NumOfRooms > 0)
            {
                mPrintRooms(screenToDebug.Controls.Where(i => i.Value.GetType() == typeof(Room)).ToList(), indent);
            }
            if (screenToDebug.NumOfPaths > 0)
            {
                mPrintPaths(screenToDebug.Controls.Where(i => i.Value.GetType() == typeof(Path)).ToList(), indent);
            }
            if (screenToDebug.NumOfWalls > 0)
            {
                mPrintWalls(screenToDebug.Controls.Where(i => i.Value.GetType() == typeof(HorizontalWall) || i.Value.GetType() == typeof(VerticalWall)).ToList(), indent);
            }
        }

        private void mPrintWalls(List<KeyValuePair<string, Component>> mWalls, int indent)
        {
            foreach (var mWall in mWalls)
            {
                VirtualConsoleAddLine(string.Format("{0}Wall {1} is located on ({2},{3}), width = {4}, height = {5}", new string(' ', indent * 2), mWall.Value.Name, mWall.Value.x, mWall.Value.y, mWall.Value.Bounds.width, mWall.Value.Bounds.height));
                VirtualConsoleAddLine(string.Format("{0}Wall contains {1} Rooms, {2} Paths, {3} walls", new string(' ', indent * 2), mWall.Value.NumOfRooms,mWall.Value.NumOfPaths, mWall.Value.NumOfWalls));
                PrintControls(mWall.Value, indent + 1);


            }
        }

        private void mPrintPaths(List<KeyValuePair<string, Component>> mPaths, int indent)
        {
            foreach (var mPath in mPaths)
            {
                VirtualConsoleAddLine(string.Format("{0}Path {1} is polynomial of {2}th power", new string(' ', indent * 2), mPath.Value.Name, (mPath.Value as Path).ConnectedComponent.Count));
                VirtualConsoleAddLine(string.Format("{0}Room contains {1} Rooms, {2} Paths, {3} walls", new string(' ', indent * 2), mPath.Value.NumOfRooms,mPath.Value.NumOfPaths, mPath.Value.NumOfWalls));
                if ((mPath.Value as Path).ConnectedComponent.Count > 0)
                {
                    mPrintRooms((mPath.Value as Path).ConnectedComponent.Where(i => i.GetType() == typeof(Room)).ToList(), indent + 1);
                }
                PrintControls(mPath.Value, indent + 1);

            }
        }

        private void mPrintRooms(List<Component> mRooms, int indent)
        {
            foreach (var mRoom in mRooms)
            {
                VirtualConsoleAddLine(string.Format("{0}Room {1} is located on ({2},{3}), width = {4}, height = {5}", new string(' ', indent * 2), mRoom.Name, mRoom.x, mRoom.y, mRoom.Bounds.width, mRoom.Bounds.height));
                VirtualConsoleAddLine(string.Format("{0}Room contains {1} Rooms, {2} Paths, {3} walls", new string(' ', indent * 2), mRoom.NumOfRooms, mRoom.NumOfPaths, mRoom.NumOfWalls));
                if (mRoom.NumOfRooms > 0)
                {
                    mPrintRooms(mRoom.Controls.Where(i => i.Value.GetType() == typeof(Room)).ToList(), indent + 1);
                }
                PrintControls(mRoom, indent + 1);

            }
        }

        private void mPrintRooms(List<KeyValuePair<string, Component>> mRooms,int indent)
        {
            foreach(var mRoom in mRooms)
            {
                VirtualConsoleAddLine(string.Format("{0}Room {1} is located on ({2},{3}), width = {4}, height = {5}",new string(' ',indent*2), mRoom.Value.Name, mRoom.Value.x, mRoom.Value.y, mRoom.Value.Bounds.width, mRoom.Value.Bounds.height));
                VirtualConsoleAddLine(string.Format("{0}Room contains {1} Rooms, {2} Paths, {3} walls",new string(' ', indent*2), mRoom.Value.NumOfRooms,mRoom.Value.NumOfPaths, mRoom.Value.NumOfWalls));
                PrintControls(mRoom.Value, indent + 1);


            }
        }
    }
}
