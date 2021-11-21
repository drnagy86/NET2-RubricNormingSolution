﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataObjects
{
    public class Rubric
    {
        public int RubricID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public String ScoreTypeID { get; set; }
        public User RubricCreator { get; set; }
        public bool Active { get; set; }

    }


    public class RubricVM : Rubric
    {
        public List<Facet> Facets { get; set; }
        public List<Criteria> Criteria { get; set; }
        public Dictionary<Facet, List<Criteria>> FacetCriteria { get; set; }
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }



        public RubricVM()
        {

        }

        public RubricVM(Rubric rubric, List<Facet> facets, List<Criteria> criteria)
        {
            this.RubricID = rubric.RubricID;
            this.Name = rubric.Name;
            this.Description = rubric.Description;
            this.DateCreated = rubric.DateCreated;
            this.DateUpdated = rubric.DateCreated;
            this.ScoreTypeID = rubric.ScoreTypeID;
            this.RubricCreator = rubric.RubricCreator;
            this.Active = rubric.Active;
            this.Facets = facets;
            this.Criteria = criteria;
            this.FacetCriteria = createFacetCriteriaDictionary(facets, criteria);

            this.ColumnCount = FacetCriteria.Count;


        }

        private Dictionary<Facet, List<Criteria>> createFacetCriteriaDictionary(List<Facet> facets, List<Criteria> criteriaList)
        {

            Dictionary<Facet, List<Criteria>> facetCriteriaDictionary = new Dictionary<Facet, List<Criteria>>();

            foreach (Facet facet in facets)
            {
                List<Criteria> criteriaForFacet = new List<Criteria>();

                foreach (Criteria criteria in criteriaList)
                {
                    if (criteria.FacetID == facet.FacetID)
                    {
                        criteriaForFacet.Add(criteria);
                    }
                }

                if (criteriaForFacet.Count == 0)
                {
                    // find out the amount of rows needed
                    Dictionary<String, int> facetCriteriaCount = new Dictionary<string, int>();
                    for (int i = 0; i < criteriaList.Count; i++)
                    {
                        if (!facetCriteriaCount.ContainsKey(criteriaList[i].FacetID))
                        {
                            facetCriteriaCount.Add(criteriaList[i].FacetID, 1);
                        }
                        else
                        {
                            facetCriteriaCount[criteriaList[i].FacetID]++;
                        }
                    }
                    this.RowCount = facetCriteriaCount.Values.Max();

                    for (int i = 0; i < this.RowCount; i++)
                    {
                        criteriaForFacet.Add(new Criteria(this.RubricID, facet.FacetID));
                    }
                }

                facetCriteriaDictionary.Add(facet, criteriaForFacet);
                //this.FacetCriteria.Add(facet, criteriaForFacet.FindAll(c => c.FacetID == facet.FacetID));               

            }

            return facetCriteriaDictionary;
        }

    }
}
