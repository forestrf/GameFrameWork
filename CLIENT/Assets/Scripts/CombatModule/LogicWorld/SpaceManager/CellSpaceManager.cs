﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Cell
    {
        public List<PositionComponent> m_entities = new List<PositionComponent>();
    }

    public class CellSpaceManager : ISpaceManager
    {
        static readonly FixPoint CELL_SIZE = FixPoint.Ten;
        static readonly FixPoint MAX_ENTITY_RADIUS = FixPoint.Two;

        LogicWorld m_logic_world;
        Vector3FP m_left_bottom_position = new Vector3FP();
        Cell[,] m_cells;
        int CELL_X_COUNT = 0;
        int CELL_Z_COUNT = 0;
        int MAX_X_INDEX = 0;
        int MAX_Z_INDEX = 0;

        List<int> m_collection = new List<int>();
        int m_min_x;
        int m_max_x;
        int m_min_z;
        int m_max_z;

        public CellSpaceManager(LogicWorld logic_world, FixPoint x_size, FixPoint z_size, Vector3FP left_bottom_position)
        {
            m_logic_world = logic_world;
            m_left_bottom_position = left_bottom_position;
            CELL_X_COUNT = (int)((x_size + CELL_SIZE - FixPoint.PrecisionFP) / CELL_SIZE);
            CELL_Z_COUNT = (int)((z_size + CELL_SIZE - FixPoint.PrecisionFP) / CELL_SIZE);
            MAX_X_INDEX = CELL_X_COUNT - 1;
            MAX_Z_INDEX = CELL_Z_COUNT - 1;
            m_cells = new Cell[CELL_X_COUNT, CELL_Z_COUNT];
            for (int x = 0; x < CELL_X_COUNT; ++x)
            {
                for (int z = 0; z < CELL_Z_COUNT; ++z)
                    m_cells[x, z] = new Cell();
            }
        }

        public void Destruct()
        {
        }

        public void AddEntiy(PositionComponent entity)
        {
            Cell cell = Position2Cell(entity.CurrentPosition);
            if (cell == null)
                return;
            cell.m_entities.Add(entity);
        }

        public void RemoveEntity(PositionComponent entity)
        {
            Cell cell = Position2Cell(entity.CurrentPosition);
            if (cell == null)
                return;
            cell.m_entities.Remove(entity);
        }

        public void UpdateEntity(PositionComponent entity, Vector3FP new_position)
        {
            Cell old_cell = Position2Cell(entity.CurrentPosition);
            Cell new_cell = Position2Cell(new_position);
            if (old_cell == new_cell)
                return;
            if (old_cell != null)
                old_cell.m_entities.Remove(entity);
            if (new_cell != null)
                new_cell.m_entities.Add(entity);
        }

        #region 帮助函数
        Cell Position2Cell(Vector3FP position)
        {
            int x_index = (int)((position.x - m_left_bottom_position.x) / CELL_SIZE);
            int z_index = (int)((position.z - m_left_bottom_position.z) / CELL_SIZE);
            return GetCell(x_index, z_index);
        }

        Cell GetCell(int x_index, int z_index)
        {
            if (x_index < 0 || x_index > MAX_X_INDEX || z_index < 0 || z_index > MAX_Z_INDEX)
                return null;
            return m_cells[x_index, z_index];
        }

        Cell GetCellUncheck(int x_index, int z_index)
        {
            return m_cells[x_index, z_index];
        }

        void ComputeAreaXZ(Vector2FP pos1, Vector2FP pos2)
        {
            pos1.x -= m_left_bottom_position.x;
            pos1.z -= m_left_bottom_position.z;
            pos2.x -= m_left_bottom_position.x;
            pos2.z -= m_left_bottom_position.z;
            if (pos1.x < pos2.x)
            {
                m_min_x = (int)(pos1.x / CELL_SIZE);
                m_max_x = (int)(pos2.x / CELL_SIZE);
            }
            else
            {
                m_min_x = (int)(pos2.x / CELL_SIZE);
                m_max_x = (int)(pos1.x / CELL_SIZE);
            }
            if (pos1.z < pos2.z)
            {
                m_min_z = (int)(pos1.z / CELL_SIZE);
                m_max_z = (int)(pos2.z / CELL_SIZE);
            }
            else
            {
                m_min_z = (int)(pos2.z / CELL_SIZE);
                m_max_z = (int)(pos1.z / CELL_SIZE);
            }
            if (m_min_x < 0)
                m_min_x = 0;
            if (m_max_x > MAX_X_INDEX)
                m_max_x = MAX_X_INDEX;
            if (m_min_z < 0)
                m_min_z = 0;
            if (m_max_z > MAX_Z_INDEX)
                m_max_z = MAX_Z_INDEX;
        }
        #endregion

        public List<int> CollectEntity_Point(Vector3FP position, int exclude_id)
        {
            m_collection.Clear();
            FixPoint position_x = position.x - m_left_bottom_position.x;
            FixPoint position_z = position.z - m_left_bottom_position.z;
            int min_x = (int)((position_x - MAX_ENTITY_RADIUS) / CELL_SIZE);
            if (min_x < 0)
                min_x = 0;
            int max_x = (int)((position_x + MAX_ENTITY_RADIUS) / CELL_SIZE);
            if (max_x > MAX_X_INDEX)
                max_x = MAX_X_INDEX;
            int min_z = (int)((position_z - MAX_ENTITY_RADIUS) / CELL_SIZE);
            if (min_z < 0)
                min_z = 0;
            int max_z = (int)((position_z + MAX_ENTITY_RADIUS) / CELL_SIZE);
            if (max_z > MAX_Z_INDEX)
                max_z = MAX_Z_INDEX;
            Cell cell;
            PositionComponent cmp;
            for (int x = min_x; x <= max_x; ++x)
            {
                for (int z = min_z; z <= max_z; ++z)
                {
                    cell = m_cells[x, z];
                    for (int i = 0; i < cell.m_entities.Count; ++i)
                    {
                        cmp = cell.m_entities[i];
                        Vector3FP offset = position - cmp.CurrentPosition;
                        if (FixPoint.FastDistance(offset.x, offset.z) < cmp.Radius)
                        {
                            int id = cmp.GetOwnerEntityID();
                            if (id != exclude_id)
                                m_collection.Add(cmp.GetOwnerEntityID());
                        }
                    }
                }
            }
            return m_collection;
        }

        public List<int> CollectEntity_ForwardArea(Vector3FP position, Vector2FP direction, FixPoint length, FixPoint width)
        {
            m_collection.Clear();
            Vector2FP start_position = new Vector2FP(position);
            Vector2FP end_position = start_position + direction * length;
            ComputeAreaXZ(start_position, end_position);
            Vector2FP side = direction.Perpendicular();
            width = width >> 1;
            Cell cell;
            PositionComponent cmp;
            Vector2FP offset;
            for (int x = m_min_x; x <= m_max_x; ++x)
            {
                for (int z = m_min_z; z <= m_max_z; ++z)
                {
                    cell = m_cells[x, z];
                    for (int i = 0; i < cell.m_entities.Count; ++i)
                    {
                        cmp = cell.m_entities[i];
                        offset.x = cmp.CurrentPosition.x - position.x;
                        offset.z = cmp.CurrentPosition.z - position.z;
                        FixPoint component = offset.Dot(ref direction);
                        FixPoint radius = cmp.Radius;
                        if (component < -radius || component > length + radius)
                            continue;
                        component = offset.Dot(ref side);
                        if (component < FixPoint.Zero)
                            component = -component;
                        if (component > width + radius)
                            continue;
                        m_collection.Add(cmp.GetOwnerEntityID());
                    }
                }
            }
            return m_collection;
        }
    }
}