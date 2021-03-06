﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    /* 
     * 时间有两种：一种是时刻，一种是时间间隔
     * 根据游戏逻辑需要，时间有基于某原点（逻辑正式启动的时刻是0）的时间（单位毫秒），和根据回合（第一个回合的的索引是1）的时间（单位回合）
     * 对持续性事件，当前回合是第一个回合（即使该回合马上就要结束），当前回合结束就是第一个回合结束
     * 配置时间时，需要配置一个时间类型+时间单位
     * 
     * 基于某原点的时间，比较简单
     * 如果要配置延迟0.5秒生效，时间就是（0，500）
     * 持续2秒，就是（0，2000）
     * 
     * 基于回合的时间，我们把他们拟化到一个持续增长的时间线，第M回合开始的时间是 M * 10，第M回合结束的时间是 M * 10 + 1，持续N个回合就是 N * 10
     * 假设当前回合是X
     * 有一个持续时间是3个游戏回合（类型是1）的事件，这个持续时间配置为（1，30）
     * 如果这个事件要立马生效，则延迟0，配置为（1，0），触发时间是 X * 10 + 0，结束时间是 X * 10 + (3-1) * 10 + 1 = X * 10 + 21
     * 如果要延迟到本回合结束，则延迟1，配置为（1，1），触发时间是 X * 10 + 1，结束时间是 X * 10 + (3-1) * 10 + 1 = X * 10 + 21
     * 延迟到下回合开始，则延迟10，配置为（1，10），触发时间是 X * 10 + 10，结束时间是 X * 10 + 31
     * 延迟到下回合结束，则延迟11，配置为（1，11），触发时间是 X * 10 + 11，结束时间是 X * 10 + 31
     * 以此类推
     * 
     * 处理task时要注意，当前时间 >= 任务时间的都要执行
     */
    class TimeType
    {
        public const int TT_NormalTime = 0;
        public const int TT_GameTurnTime = 1;
        public const int TT_PlayerTurnTime = 2;
        public const int TT_ObjectTurnTime = 3;

        static int TurnTime2Int(int time_type, int value)
        {
            return 0;
        }

        static int Int2TurnTime()
        {
            return 0;
        }
    }
}